using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Ports;

namespace Hammer.Collector.Application.UseCases.Analytics.GetLatencyAnalysis;

internal sealed class GetLatencyAnalysisUseCase(IAnalyticsRepository analyticsRepository) : IGetLatencyAnalysisUseCase
{
    public async Task<LatencyAnalysisResult> ExecuteAsync(GetLatencyAnalysisRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.From >= request.To)
            throw new BadRequestException("'From' must be earlier than 'To'.");

        return await analyticsRepository.GetLatencyAnalysisAsync(request.From, request.To, ct);
    }
}
