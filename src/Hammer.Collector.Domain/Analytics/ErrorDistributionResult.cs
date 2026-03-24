namespace Hammer.Collector.Domain.Analytics;

public sealed record ErrorDistributionResult(
    IReadOnlyList<ErrorDistributionEntry> ByExceptionType,
    IReadOnlyList<ErrorDistributionEntry> BySource,
    IReadOnlyList<ErrorDistributionEntry> ByLevel);
