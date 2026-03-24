using Hammer.Collector.Domain.Search;

namespace Hammer.Collector.Application.UseCases.Search.SearchByTraceId;

public interface ISearchByTraceIdUseCase
{
    public Task<TraceSearchResult> ExecuteAsync(SearchByTraceIdRequest request, CancellationToken ct);
}
