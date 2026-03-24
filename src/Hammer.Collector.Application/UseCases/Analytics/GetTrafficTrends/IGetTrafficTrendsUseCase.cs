using Hammer.Collector.Domain.Analytics;

namespace Hammer.Collector.Application.UseCases.Analytics.GetTrafficTrends;

public interface IGetTrafficTrendsUseCase
{
    public Task<TrafficTrendResult> ExecuteAsync(GetTrafficTrendsRequest request, CancellationToken ct);
}
