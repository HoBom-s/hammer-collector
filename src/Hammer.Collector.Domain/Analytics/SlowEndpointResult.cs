namespace Hammer.Collector.Domain.Analytics;

public sealed record SlowEndpointResult(IReadOnlyList<SlowEndpointEntry> Endpoints);
