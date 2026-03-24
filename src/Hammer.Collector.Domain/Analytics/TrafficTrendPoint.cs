namespace Hammer.Collector.Domain.Analytics;

public sealed record TrafficTrendPoint(
    DateTimeOffset Bucket,
    long RequestCount,
    double Rps);
