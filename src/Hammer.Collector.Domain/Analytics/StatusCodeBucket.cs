namespace Hammer.Collector.Domain.Analytics;

public sealed record StatusCodeBucket(
    DateTimeOffset Bucket,
    int StatusCodeClass,
    long Count);
