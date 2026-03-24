using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Ports;

namespace Hammer.Collector.Application.UseCases.Analytics.GetStatusCodeDistribution;

internal sealed class GetStatusCodeDistributionUseCase(IAnalyticsRepository analyticsRepository) : IGetStatusCodeDistributionUseCase
{
    public async Task<StatusCodeDistributionResult> ExecuteAsync(GetStatusCodeDistributionRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.From >= request.To)
            throw new BadRequestException("'From' must be earlier than 'To'.");

        return await analyticsRepository.GetStatusCodeDistributionAsync(request.From, request.To, request.Bucket, ct);
    }
}
