using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Ports;

namespace Hammer.Collector.Application.UseCases.Analytics.GetErrorDistribution;

internal sealed class GetErrorDistributionUseCase(IAnalyticsRepository analyticsRepository) : IGetErrorDistributionUseCase
{
    public async Task<ErrorDistributionResult> ExecuteAsync(GetErrorDistributionRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.From >= request.To)
            throw new BadRequestException("'From' must be earlier than 'To'.");

        return await analyticsRepository.GetErrorDistributionAsync(request.From, request.To, ct);
    }
}
