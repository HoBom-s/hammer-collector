namespace Hammer.Collector.Domain.Analytics;

public sealed record LatencyAnalysisResult(
    double AvgMs,
    long MaxMs,
    double P50Ms,
    double P95Ms,
    double P99Ms,
    long TotalRequests);
