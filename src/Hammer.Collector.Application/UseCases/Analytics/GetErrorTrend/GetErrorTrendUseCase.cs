using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Ports;

namespace Hammer.Collector.Application.UseCases.Analytics.GetErrorTrend;

internal sealed class GetErrorTrendUseCase(IAnalyticsRepository analyticsRepository) : IGetErrorTrendUseCase
{
    public async Task<ErrorTrendResult> ExecuteAsync(GetErrorTrendRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.From >= request.To)
            throw new BadRequestException("'From' must be earlier than 'To'.");

        return await analyticsRepository.GetErrorTrendAsync(request.From, request.To, request.Bucket, ct);
    }
}
