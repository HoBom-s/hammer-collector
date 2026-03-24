using Hammer.Collector.Domain.Analytics;

namespace Hammer.Collector.Application.UseCases.Analytics.GetErrorDistribution;

public interface IGetErrorDistributionUseCase
{
    public Task<ErrorDistributionResult> ExecuteAsync(GetErrorDistributionRequest request, CancellationToken ct);
}
