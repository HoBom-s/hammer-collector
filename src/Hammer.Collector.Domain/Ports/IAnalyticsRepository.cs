using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Enums;

namespace Hammer.Collector.Domain.Ports;

public interface IAnalyticsRepository
{
    public Task<TrafficTrendResult> GetTrafficTrendsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        TimeBucket bucket,
        CancellationToken ct = default);

    public Task<StatusCodeDistributionResult> GetStatusCodeDistributionAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        TimeBucket bucket,
        CancellationToken ct = default);

    public Task<LatencyAnalysisResult> GetLatencyAnalysisAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct = default);

    public Task<SlowEndpointResult> GetSlowEndpointsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int top,
        CancellationToken ct = default);

    public Task<ErrorTrendResult> GetErrorTrendAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        TimeBucket bucket,
        CancellationToken ct = default);

    public Task<ErrorDistributionResult> GetErrorDistributionAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct = default);

    public Task<ErrorListResult> GetRecentErrorsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
