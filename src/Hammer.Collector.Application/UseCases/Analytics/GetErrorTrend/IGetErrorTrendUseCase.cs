using Hammer.Collector.Domain.Analytics;

namespace Hammer.Collector.Application.UseCases.Analytics.GetErrorTrend;

public interface IGetErrorTrendUseCase
{
    public Task<ErrorTrendResult> ExecuteAsync(GetErrorTrendRequest request, CancellationToken ct);
}
