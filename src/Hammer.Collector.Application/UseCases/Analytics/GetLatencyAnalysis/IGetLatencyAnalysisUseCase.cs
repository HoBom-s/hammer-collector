using Hammer.Collector.Domain.Analytics;

namespace Hammer.Collector.Application.UseCases.Analytics.GetLatencyAnalysis;

public interface IGetLatencyAnalysisUseCase
{
    public Task<LatencyAnalysisResult> ExecuteAsync(GetLatencyAnalysisRequest request, CancellationToken ct);
}
