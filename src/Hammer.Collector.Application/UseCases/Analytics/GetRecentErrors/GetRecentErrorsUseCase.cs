using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Ports;

namespace Hammer.Collector.Application.UseCases.Analytics.GetRecentErrors;

internal sealed class GetRecentErrorsUseCase(IAnalyticsRepository analyticsRepository) : IGetRecentErrorsUseCase
{
    public async Task<ErrorListResult> ExecuteAsync(GetRecentErrorsRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.From >= request.To)
            throw new BadRequestException("'From' must be earlier than 'To'.");

        if (request.Page < 1)
            throw new BadRequestException("'Page' must be at least 1.");

        if (request.PageSize <= 0)
            throw new BadRequestException("'PageSize' must be greater than 0.");

        return await analyticsRepository.GetRecentErrorsAsync(request.From, request.To, request.Page, request.PageSize, ct);
    }
}
