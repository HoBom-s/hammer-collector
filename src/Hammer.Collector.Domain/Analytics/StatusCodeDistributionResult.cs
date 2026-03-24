namespace Hammer.Collector.Domain.Analytics;

public sealed record StatusCodeDistributionResult(
    IReadOnlyList<StatusCodeSummary> Summary,
    IReadOnlyList<StatusCodeBucket> TimeSeries);
