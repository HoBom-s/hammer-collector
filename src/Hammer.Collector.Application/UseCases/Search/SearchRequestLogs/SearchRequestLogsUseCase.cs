using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Domain.Ports;
using Hammer.Collector.Domain.Search;

namespace Hammer.Collector.Application.UseCases.Search.SearchRequestLogs;

internal sealed class SearchRequestLogsUseCase(ISearchRepository searchRepository) : ISearchRequestLogsUseCase
{
    public async Task<RequestLogSearchResult> ExecuteAsync(SearchRequestLogsRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.From >= request.To)
            throw new BadRequestException("'From' must be earlier than 'To'.");

        if (request.Page < 1)
            throw new BadRequestException("'Page' must be at least 1.");

        if (request.PageSize <= 0)
            throw new BadRequestException("'PageSize' must be greater than 0.");

        return await searchRepository.SearchRequestLogsAsync(
            request.From,
            request.To,
            request.TraceId,
            request.StatusCode,
            request.Method,
            request.Path,
            request.Page,
            request.PageSize,
            ct);
    }
}
