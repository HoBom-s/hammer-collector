using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Domain.Ports;
using Hammer.Collector.Domain.Search;

namespace Hammer.Collector.Application.UseCases.Search.SearchByTraceId;

internal sealed class SearchByTraceIdUseCase(ISearchRepository searchRepository) : ISearchByTraceIdUseCase
{
    public async Task<TraceSearchResult> ExecuteAsync(SearchByTraceIdRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.TraceId))
            throw new BadRequestException("'TraceId' must not be empty.");

        return await searchRepository.SearchByTraceIdAsync(request.TraceId, ct);
    }
}
