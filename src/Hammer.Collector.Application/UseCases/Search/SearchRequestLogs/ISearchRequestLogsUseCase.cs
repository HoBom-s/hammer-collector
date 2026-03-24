using Hammer.Collector.Domain.Search;

namespace Hammer.Collector.Application.UseCases.Search.SearchRequestLogs;

public interface ISearchRequestLogsUseCase
{
    public Task<RequestLogSearchResult> ExecuteAsync(SearchRequestLogsRequest request, CancellationToken ct);
}
