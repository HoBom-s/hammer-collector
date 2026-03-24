using Hammer.Collector.Domain.Analytics;

namespace Hammer.Collector.Application.UseCases.Analytics.GetStatusCodeDistribution;

public interface IGetStatusCodeDistributionUseCase
{
    public Task<StatusCodeDistributionResult> ExecuteAsync(GetStatusCodeDistributionRequest request, CancellationToken ct);
}
