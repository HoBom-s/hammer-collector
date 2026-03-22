namespace Hammer.Collector.Domain.Entities;

public sealed class GatewayRequestLog
{
    public long Id { get; init; }

    public required string TraceId { get; init; }

    public string? UserId { get; init; }

    public required string Method { get; init; }

    public required string Path { get; init; }

    public string? QueryString { get; init; }

    public int StatusCode { get; init; }

    public long DurationMs { get; init; }

    public string? ClientIp { get; init; }

    public string? UserAgent { get; init; }

    public long? RequestSize { get; init; }

    public long? ResponseSize { get; init; }

    public string? RouteCluster { get; init; }

    public DateTimeOffset Timestamp { get; init; }
}
