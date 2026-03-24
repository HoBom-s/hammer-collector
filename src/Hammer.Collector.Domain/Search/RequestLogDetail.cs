namespace Hammer.Collector.Domain.Search;

public sealed record RequestLogDetail(
    long Id,
    string TraceId,
    string? UserId,
    string Method,
    string Path,
    string? QueryString,
    int StatusCode,
    long DurationMs,
    string? ClientIp,
    string? UserAgent,
    string? RouteCluster,
    DateTimeOffset Timestamp);
