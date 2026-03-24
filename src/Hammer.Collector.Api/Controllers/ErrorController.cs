using Hammer.Collector.Application.UseCases.Analytics.GetErrorDistribution;
using Hammer.Collector.Application.UseCases.Analytics.GetErrorTrend;
using Hammer.Collector.Application.UseCases.Analytics.GetRecentErrors;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Hammer.Collector.Api.Controllers;

[ApiController]
[Route("analytics/errors")]
[Tags("Errors")]
[EnableRateLimiting("analytics")]
public sealed class ErrorController(
    IGetErrorTrendUseCase getErrorTrendUseCase,
    IGetErrorDistributionUseCase getErrorDistributionUseCase,
    IGetRecentErrorsUseCase getRecentErrorsUseCase) : ControllerBase
{
    [HttpGet("trend")]
    [ProducesResponseType(typeof(ErrorTrendResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrendAsync(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] TimeBucket? bucket,
        CancellationToken ct)
    {
        var request = new GetErrorTrendRequest(from, to, bucket ?? TimeBucket.Hour);
        return Ok(await getErrorTrendUseCase.ExecuteAsync(request, ct));
    }

    [HttpGet("distribution")]
    [ProducesResponseType(typeof(ErrorDistributionResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDistributionAsync(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken ct)
    {
        var request = new GetErrorDistributionRequest(from, to);
        return Ok(await getErrorDistributionUseCase.ExecuteAsync(request, ct));
    }

    [HttpGet("recent")]
    [ProducesResponseType(typeof(ErrorListResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentAsync(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var request = new GetRecentErrorsRequest(from, to, page ?? 1, pageSize ?? 20);
        return Ok(await getRecentErrorsUseCase.ExecuteAsync(request, ct));
    }
}
