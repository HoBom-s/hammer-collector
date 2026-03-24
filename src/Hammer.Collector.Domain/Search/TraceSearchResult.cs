using Hammer.Collector.Domain.Analytics;

namespace Hammer.Collector.Domain.Search;

public sealed record TraceSearchResult(
    IReadOnlyList<RequestLogDetail> RequestLogs,
    IReadOnlyList<ErrorDetail> ErrorLogs);
