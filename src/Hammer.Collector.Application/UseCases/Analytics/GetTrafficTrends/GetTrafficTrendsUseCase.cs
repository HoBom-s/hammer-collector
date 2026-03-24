using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Ports;

namespace Hammer.Collector.Application.UseCases.Analytics.GetTrafficTrends;

internal sealed class GetTrafficTrendsUseCase(IAnalyticsRepository analyticsRepository) : IGetTrafficTrendsUseCase
{
    public async Task<TrafficTrendResult> ExecuteAsync(GetTrafficTrendsRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.From >= request.To)
            throw new BadRequestException("'From' must be earlier than 'To'.");

        return await analyticsRepository.GetTrafficTrendsAsync(request.From, request.To, request.Bucket, ct);
    }
}
