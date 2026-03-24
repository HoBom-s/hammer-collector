using Hammer.Collector.Domain.Analytics;

namespace Hammer.Collector.Application.UseCases.Analytics.GetSlowEndpoints;

public interface IGetSlowEndpointsUseCase
{
    public Task<SlowEndpointResult> ExecuteAsync(GetSlowEndpointsRequest request, CancellationToken ct);
}
