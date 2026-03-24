namespace Hammer.Collector.Domain.Entities;

public sealed class ServiceErrorLog
{
    public long Id { get; init; }

    public required string TraceId { get; init; }

    public required string Source { get; init; }

    public required string Level { get; init; }

    public required string ExceptionType { get; init; }

    public required string Message { get; init; }

    public string? StackTrace { get; init; }

    public required string RequestPath { get; init; }

    public required string RequestMethod { get; init; }

    public DateTimeOffset Timestamp { get; init; }
}
