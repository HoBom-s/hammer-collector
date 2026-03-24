using System.Text.Json;
using Hammer.Collector.Domain.Entities;
using Hammer.Collector.Infrastructure.Persistence;

namespace Hammer.Collector.Infrastructure.Kafka;

internal sealed class ServiceErrorLogHandler : IKafkaMessageHandler
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public string Topic => "service-error-log";

    public void Handle(IReadOnlyList<string> messages, CollectorDbContext db)
    {
        foreach (var message in messages)
        {
            ServiceErrorLog? log = JsonSerializer.Deserialize<ServiceErrorLog>(message, _jsonOptions);

            if (log is not null)
                db.ServiceErrorLogs.Add(log);
        }
    }
}
