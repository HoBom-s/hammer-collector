using System.Diagnostics.CodeAnalysis;
using Confluent.Kafka;
using Hammer.Collector.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hammer.Collector.Infrastructure.Kafka;

[ExcludeFromCodeCoverage]
internal sealed partial class KafkaConsumerWorker : BackgroundService
{
    private const int BatchSize = 100;

    private static readonly TimeSpan _consumeTimeout = TimeSpan.FromMilliseconds(100);

    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumerWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Dictionary<string, IKafkaMessageHandler> _handlers;

    public KafkaConsumerWorker(
        IServiceScopeFactory scopeFactory,
        IEnumerable<IKafkaMessageHandler> handlers,
        IConfiguration configuration,
        ILogger<KafkaConsumerWorker> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _scopeFactory = scopeFactory;
        _logger = logger;
        _handlers = handlers.ToDictionary(h => h.Topic);

        if (_handlers.Count == 0)
            throw new InvalidOperationException("No Kafka message handlers registered.");

        var bootstrapServers = configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
            throw new InvalidOperationException("Kafka:BootstrapServers is not configured.");

        ConsumerConfig config = new()
        {
            BootstrapServers = bootstrapServers,
            GroupId = "hammer-collector",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topics = _handlers.Keys.ToList();
        var topicList = string.Join(", ", topics);
        _consumer.Subscribe(topics);
        LogSubscribed(_logger, topicList);

        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                List<ConsumeResult<string, string>> batch = ConsumeBatch(stoppingToken);

                if (batch.Count == 0)
                    continue;

                await ProcessBatchAsync(batch, stoppingToken);
                _consumer.Commit();
                LogBatchCommitted(_logger, batch.Count);
            }
            catch (OperationCanceledException)
            {
                break;
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _logger.LogError(ex, "Error consuming Kafka message batch");
            }
        }

        _consumer.Close();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Kafka consumer subscribed to topics: {Topics}")]
    private static partial void LogSubscribed(ILogger logger, string topics);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Committed batch of {Count} messages")]
    private static partial void LogBatchCommitted(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unknown topic: {Topic}")]
    private static partial void LogUnknownTopic(ILogger logger, string topic);

    private List<ConsumeResult<string, string>> ConsumeBatch(CancellationToken ct)
    {
        List<ConsumeResult<string, string>> batch = [];

        for (var i = 0; i < BatchSize; i++)
        {
            ConsumeResult<string, string>? result = i == 0
                ? _consumer.Consume(ct)
                : _consumer.Consume(_consumeTimeout);

            if (result is null)
                break;

            batch.Add(result);
        }

        return batch;
    }

    private async Task ProcessBatchAsync(List<ConsumeResult<string, string>> batch, CancellationToken ct)
    {
        Dictionary<string, List<string>> grouped = [];

        foreach (ConsumeResult<string, string> result in batch)
        {
            if (!_handlers.ContainsKey(result.Topic))
            {
                LogUnknownTopic(_logger, result.Topic);
                continue;
            }

            if (!grouped.TryGetValue(result.Topic, out List<string>? messages))
            {
                messages = [];
                grouped[result.Topic] = messages;
            }

            messages.Add(result.Message.Value);
        }

        if (grouped.Count == 0)
            return;

        using IServiceScope scope = _scopeFactory.CreateScope();
        CollectorDbContext dbContext = scope.ServiceProvider.GetRequiredService<CollectorDbContext>();

        foreach ((var topic, List<string> messages) in grouped)
            _handlers[topic].Handle(messages, dbContext);

        await dbContext.SaveChangesAsync(ct);
    }
}
