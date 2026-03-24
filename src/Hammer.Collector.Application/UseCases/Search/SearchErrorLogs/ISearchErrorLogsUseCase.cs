using Hammer.Collector.Domain.Analytics;

namespace Hammer.Collector.Application.UseCases.Search.SearchErrorLogs;

public interface ISearchErrorLogsUseCase
{
    public Task<ErrorListResult> ExecuteAsync(SearchErrorLogsRequest request, CancellationToken ct);
}
