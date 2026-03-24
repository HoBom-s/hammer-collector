using Hammer.Collector.Infrastructure.Persistence;

namespace Hammer.Collector.Infrastructure.Kafka;

internal interface IKafkaMessageHandler
{
    public string Topic { get; }

    public void Handle(IReadOnlyList<string> messages, CollectorDbContext db);
}
