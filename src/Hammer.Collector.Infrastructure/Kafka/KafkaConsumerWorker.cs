using System.Text.Json;
using Confluent.Kafka;
using Hammer.Collector.Domain.Entities;
using Hammer.Collector.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hammer.Collector.Infrastructure.Kafka;

public sealed partial class KafkaConsumerWorker : BackgroundService
{
    private const string GatewayRequestLogTopic = "gateway-request-log";
    private const string ServiceErrorLogTopic = "service-error-log";

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumerWorker> _logger;

    private readonly IServiceScopeFactory _scopeFactory;

    public KafkaConsumerWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<KafkaConsumerWorker> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _scopeFactory = scopeFactory;
        _logger = logger;

        ConsumerConfig config = new()
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "hammer-collector",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
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
        _consumer.Subscribe([GatewayRequestLogTopic, ServiceErrorLogTopic]);
        LogSubscribed(_logger, $"{GatewayRequestLogTopic}, {ServiceErrorLogTopic}");

        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ConsumeResult<string, string> result = _consumer.Consume(stoppingToken);
                await ProcessMessageAsync(result, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _logger.LogError(ex, "Error consuming Kafka message");
            }
        }

        _consumer.Close();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Kafka consumer subscribed to topics: {Topics}")]
    private static partial void LogSubscribed(ILogger logger, string topics);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Saved gateway request log: {TraceId}")]
    private static partial void LogGatewayRequestSaved(ILogger logger, string traceId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Saved service error log: {TraceId}")]
    private static partial void LogServiceErrorSaved(ILogger logger, string traceId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unknown topic: {Topic}")]
    private static partial void LogUnknownTopic(ILogger logger, string topic);

    private async Task ProcessMessageAsync(ConsumeResult<string, string> result, CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        CollectorDbContext dbContext = scope.ServiceProvider.GetRequiredService<CollectorDbContext>();

        switch (result.Topic)
        {
            case GatewayRequestLogTopic:
                GatewayRequestLog? requestLog = JsonSerializer.Deserialize<GatewayRequestLog>(result.Message.Value, _jsonOptions);

                if (requestLog is not null)
                {
                    dbContext.GatewayRequestLogs.Add(requestLog);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    LogGatewayRequestSaved(_logger, requestLog.TraceId);
                }

                break;

            case ServiceErrorLogTopic:
                ServiceErrorLog? errorLog = JsonSerializer.Deserialize<ServiceErrorLog>(result.Message.Value, _jsonOptions);

                if (errorLog is not null)
                {
                    dbContext.ServiceErrorLogs.Add(errorLog);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    LogServiceErrorSaved(_logger, errorLog.TraceId);
                }

                break;

            default:
                LogUnknownTopic(_logger, result.Topic);
                break;
        }
    }
}
