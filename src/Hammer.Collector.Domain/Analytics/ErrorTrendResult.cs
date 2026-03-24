namespace Hammer.Collector.Domain.Analytics;

public sealed record ErrorTrendResult(IReadOnlyList<ErrorTrendPoint> Points);
