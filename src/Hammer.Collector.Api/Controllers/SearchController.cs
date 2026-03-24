using Hammer.Collector.Application.UseCases.Search.SearchByTraceId;
using Hammer.Collector.Application.UseCases.Search.SearchErrorLogs;
using Hammer.Collector.Application.UseCases.Search.SearchRequestLogs;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Hammer.Collector.Api.Controllers;

[ApiController]
[Route("search")]
[Tags("Search")]
[EnableRateLimiting("analytics")]
public sealed class SearchController(
    ISearchRequestLogsUseCase searchRequestLogsUseCase,
    ISearchErrorLogsUseCase searchErrorLogsUseCase,
    ISearchByTraceIdUseCase searchByTraceIdUseCase) : ControllerBase
{
    [HttpGet("requests")]
    [ProducesResponseType(typeof(RequestLogSearchResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchRequestsAsync(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] string? traceId,
        [FromQuery] int? statusCode,
        [FromQuery] string? method,
        [FromQuery] string? path,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var request = new SearchRequestLogsRequest(
            from,
            to,
            traceId,
            statusCode,
            method,
            path,
            page ?? 1,
            pageSize ?? 20);
        return Ok(await searchRequestLogsUseCase.ExecuteAsync(request, ct));
    }

    [HttpGet("errors")]
    [ProducesResponseType(typeof(ErrorListResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchErrorsAsync(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] string? traceId,
        [FromQuery] string? exceptionType,
        [FromQuery] string? source,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var request = new SearchErrorLogsRequest(
            from,
            to,
            traceId,
            exceptionType,
            source,
            page ?? 1,
            pageSize ?? 20);
        return Ok(await searchErrorLogsUseCase.ExecuteAsync(request, ct));
    }

    [HttpGet("trace/{traceId}")]
    [ProducesResponseType(typeof(TraceSearchResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchByTraceIdAsync(
        string traceId,
        CancellationToken ct)
    {
        var request = new SearchByTraceIdRequest(traceId);
        return Ok(await searchByTraceIdUseCase.ExecuteAsync(request, ct));
    }
}
