using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Ports;

namespace Hammer.Collector.Application.UseCases.Analytics.GetSlowEndpoints;

internal sealed class GetSlowEndpointsUseCase(IAnalyticsRepository analyticsRepository) : IGetSlowEndpointsUseCase
{
    public async Task<SlowEndpointResult> ExecuteAsync(GetSlowEndpointsRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.From >= request.To)
            throw new BadRequestException("'From' must be earlier than 'To'.");

        if (request.Top <= 0)
            throw new BadRequestException("'Top' must be greater than 0.");

        return await analyticsRepository.GetSlowEndpointsAsync(request.From, request.To, request.Top, ct);
    }
}
