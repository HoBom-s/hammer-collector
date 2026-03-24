namespace Hammer.Collector.Domain.Analytics;

public sealed record ErrorDistributionEntry(
    string Key,
    long Count);
