namespace Hammer.Collector.Domain.Analytics;

public sealed record ErrorDetail(
    long Id,
    string TraceId,
    string Source,
    string Level,
    string ExceptionType,
    string Message,
    string? StackTrace,
    string RequestPath,
    string RequestMethod,
    DateTimeOffset Timestamp);
