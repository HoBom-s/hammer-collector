namespace Hammer.Collector.Domain.Analytics;

public sealed record SlowEndpointEntry(
    string Method,
    string Path,
    double AvgMs,
    long MaxMs,
    long RequestCount);
