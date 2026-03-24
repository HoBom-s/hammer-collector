namespace Hammer.Collector.Domain.Analytics;

public sealed record ErrorTrendPoint(
    DateTimeOffset Bucket,
    long ErrorCount);
