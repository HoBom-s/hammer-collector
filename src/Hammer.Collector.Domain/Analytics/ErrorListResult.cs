namespace Hammer.Collector.Domain.Analytics;

public sealed record ErrorListResult(
    IReadOnlyList<ErrorDetail> Errors,
    long TotalCount,
    int Page,
    int PageSize);
