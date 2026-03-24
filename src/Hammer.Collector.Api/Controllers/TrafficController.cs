using Hammer.Collector.Application.UseCases.Analytics.GetLatencyAnalysis;
using Hammer.Collector.Application.UseCases.Analytics.GetSlowEndpoints;
using Hammer.Collector.Application.UseCases.Analytics.GetStatusCodeDistribution;
using Hammer.Collector.Application.UseCases.Analytics.GetTrafficTrends;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Collector.Api.Controllers;

[ApiController]
[Route("analytics/traffic")]
[Tags("Traffic")]
public sealed class TrafficController(
    IGetTrafficTrendsUseCase getTrafficTrendsUseCase,
    IGetStatusCodeDistributionUseCase getStatusCodeDistributionUseCase,
    IGetLatencyAnalysisUseCase getLatencyAnalysisUseCase,
    IGetSlowEndpointsUseCase getSlowEndpointsUseCase) : ControllerBase
{
    [HttpGet("trends")]
    [ProducesResponseType(typeof(TrafficTrendResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrendsAsync(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] TimeBucket? bucket,
        CancellationToken ct)
    {
        var request = new GetTrafficTrendsRequest(from, to, bucket ?? TimeBucket.Hour);
        return Ok(await getTrafficTrendsUseCase.ExecuteAsync(request, ct));
    }

    [HttpGet("status-codes")]
    [ProducesResponseType(typeof(StatusCodeDistributionResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatusCodesAsync(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] TimeBucket? bucket,
        CancellationToken ct)
    {
        var request = new GetStatusCodeDistributionRequest(from, to, bucket ?? TimeBucket.Hour);
        return Ok(await getStatusCodeDistributionUseCase.ExecuteAsync(request, ct));
    }

    [HttpGet("latency")]
    [ProducesResponseType(typeof(LatencyAnalysisResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatencyAsync(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken ct)
    {
        var request = new GetLatencyAnalysisRequest(from, to);
        return Ok(await getLatencyAnalysisUseCase.ExecuteAsync(request, ct));
    }

    [HttpGet("latency/slow-endpoints")]
    [ProducesResponseType(typeof(SlowEndpointResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSlowEndpointsAsync(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] int? top,
        CancellationToken ct)
    {
        var request = new GetSlowEndpointsRequest(from, to, top ?? 10);
        return Ok(await getSlowEndpointsUseCase.ExecuteAsync(request, ct));
    }
}
