using System.Text.Json;
using Hammer.Collector.Domain.Entities;
using Hammer.Collector.Infrastructure.Persistence;

namespace Hammer.Collector.Infrastructure.Kafka;

internal sealed class GatewayRequestLogHandler : IKafkaMessageHandler
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public string Topic => "gateway-request-log";

    public void Handle(IReadOnlyList<string> messages, CollectorDbContext db)
    {
        foreach (var message in messages)
        {
            GatewayRequestLog? log = JsonSerializer.Deserialize<GatewayRequestLog>(message, _jsonOptions);

            if (log is not null)
                db.GatewayRequestLogs.Add(log);
        }
    }
}
