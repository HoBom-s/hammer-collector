using Hammer.Collector.Domain.Analytics;

namespace Hammer.Collector.Application.UseCases.Analytics.GetRecentErrors;

public interface IGetRecentErrorsUseCase
{
    public Task<ErrorListResult> ExecuteAsync(GetRecentErrorsRequest request, CancellationToken ct);
}
