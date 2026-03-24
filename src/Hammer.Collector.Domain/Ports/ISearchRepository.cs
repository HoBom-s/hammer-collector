using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Search;

namespace Hammer.Collector.Domain.Ports;

public interface ISearchRepository
{
    public Task<RequestLogSearchResult> SearchRequestLogsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        string? traceId,
        int? statusCode,
        string? method,
        string? path,
        int page,
        int pageSize,
        CancellationToken ct = default);

    public Task<ErrorListResult> SearchErrorLogsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        string? traceId,
        string? exceptionType,
        string? source,
        int page,
        int pageSize,
        CancellationToken ct = default);

    public Task<TraceSearchResult> SearchByTraceIdAsync(
        string traceId,
        CancellationToken ct = default);
}
