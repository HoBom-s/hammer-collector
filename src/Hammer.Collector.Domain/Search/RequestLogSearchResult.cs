namespace Hammer.Collector.Domain.Search;

public sealed record RequestLogSearchResult(
    IReadOnlyList<RequestLogDetail> Logs,
    long TotalCount,
    int Page,
    int PageSize);
