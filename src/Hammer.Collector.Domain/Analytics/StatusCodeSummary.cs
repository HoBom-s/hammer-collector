namespace Hammer.Collector.Domain.Analytics;

public sealed record StatusCodeSummary(
    int StatusCodeClass,
    long TotalCount,
    double Percentage);
