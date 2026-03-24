using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hammer.Collector.Application.UseCases.Analytics.GetLatencyAnalysis;
using Hammer.Collector.Application.UseCases.Analytics.GetSlowEndpoints;
using Hammer.Collector.Application.UseCases.Analytics.GetStatusCodeDistribution;
using Hammer.Collector.Application.UseCases.Analytics.GetTrafficTrends;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Enums;
using Hammer.Collector.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Hammer.Collector.Tests.Api.Controllers;

public sealed class TrafficControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly DateTimeOffset _from = new(2026, 3, 22, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _to = new(2026, 3, 23, 0, 0, 0, TimeSpan.Zero);
    private static readonly string _fromEncoded = Uri.EscapeDataString(_from.ToString("O"));
    private static readonly string _toEncoded = Uri.EscapeDataString(_to.ToString("O"));

    private readonly WebApplicationFactory<Program> _factory;

    public TrafficControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTrends_ShouldReturn200WithPointsAsync()
    {
        // Arrange
        IGetTrafficTrendsUseCase useCase = Substitute.For<IGetTrafficTrendsUseCase>();
        useCase.ExecuteAsync(Arg.Any<GetTrafficTrendsRequest>(), Arg.Any<CancellationToken>())
            .Returns(new TrafficTrendResult([
                new TrafficTrendPoint(_from, 100, 0.03),
                new TrafficTrendPoint(_from.AddHours(1), 200, 0.06),
            ]));

        HttpClient client = CreateClient(trafficTrends: useCase);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri($"/analytics/traffic/trends?from={_fromEncoded}&to={_toEncoded}", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TrafficTrendResult? body = await response.Content.ReadFromJsonAsync<TrafficTrendResult>();
        body!.Points.Should().HaveCount(2);
        body.Points[0].RequestCount.Should().Be(100);
        body.Points[1].RequestCount.Should().Be(200);
    }

    [Fact]
    public async Task GetTrends_ShouldPassBucketParameterAsync()
    {
        // Arrange
        IGetTrafficTrendsUseCase useCase = Substitute.For<IGetTrafficTrendsUseCase>();
        useCase.ExecuteAsync(Arg.Any<GetTrafficTrendsRequest>(), Arg.Any<CancellationToken>())
            .Returns(new TrafficTrendResult([]));

        HttpClient client = CreateClient(trafficTrends: useCase);

        // Act
        await client.GetAsync(
            new Uri($"/analytics/traffic/trends?from={_fromEncoded}&to={_toEncoded}&bucket=Day", UriKind.Relative));

        // Assert
        await useCase.Received(1).ExecuteAsync(
            Arg.Is<GetTrafficTrendsRequest>(r => r.Bucket == TimeBucket.Day),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStatusCodes_ShouldReturn200WithDistributionAsync()
    {
        // Arrange
        IGetStatusCodeDistributionUseCase useCase = Substitute.For<IGetStatusCodeDistributionUseCase>();
        useCase.ExecuteAsync(Arg.Any<GetStatusCodeDistributionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new StatusCodeDistributionResult(
                [new StatusCodeSummary(2, 80, 80.0), new StatusCodeSummary(5, 20, 20.0)],
                [new StatusCodeBucket(_from, 2, 80)]));

        HttpClient client = CreateClient(statusCodeDistribution: useCase);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri($"/analytics/traffic/status-codes?from={_fromEncoded}&to={_toEncoded}", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        StatusCodeDistributionResult? body = await response.Content
            .ReadFromJsonAsync<StatusCodeDistributionResult>();
        body!.Summary.Should().HaveCount(2);
        body.Summary[0].Percentage.Should().Be(80.0);
    }

    [Fact]
    public async Task GetLatency_ShouldReturn200WithPercentilesAsync()
    {
        // Arrange
        IGetLatencyAnalysisUseCase useCase = Substitute.For<IGetLatencyAnalysisUseCase>();
        useCase.ExecuteAsync(Arg.Any<GetLatencyAnalysisRequest>(), Arg.Any<CancellationToken>())
            .Returns(new LatencyAnalysisResult(176.0, 500, 100.0, 440.0, 488.0, 5));

        HttpClient client = CreateClient(latencyAnalysis: useCase);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri($"/analytics/traffic/latency?from={_fromEncoded}&to={_toEncoded}", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        LatencyAnalysisResult? body = await response.Content.ReadFromJsonAsync<LatencyAnalysisResult>();
        body!.P50Ms.Should().Be(100.0);
        body.P95Ms.Should().Be(440.0);
        body.MaxMs.Should().Be(500);
        body.TotalRequests.Should().Be(5);
    }

    [Fact]
    public async Task GetSlowEndpoints_ShouldReturn200WithTopNAsync()
    {
        // Arrange
        IGetSlowEndpointsUseCase useCase = Substitute.For<IGetSlowEndpointsUseCase>();
        useCase.ExecuteAsync(Arg.Any<GetSlowEndpointsRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SlowEndpointResult([
                new SlowEndpointEntry("GET", "/api/users", 216.67, 500, 3),
                new SlowEndpointEntry("POST", "/api/users", 200.0, 200, 1),
            ]));

        HttpClient client = CreateClient(slowEndpoints: useCase);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri($"/analytics/traffic/latency/slow-endpoints?from={_fromEncoded}&to={_toEncoded}&top=2", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        SlowEndpointResult? body = await response.Content.ReadFromJsonAsync<SlowEndpointResult>();
        body!.Endpoints.Should().HaveCount(2);
        body.Endpoints[0].AvgMs.Should().BeGreaterThan(body.Endpoints[1].AvgMs);

        await useCase.Received(1).ExecuteAsync(
            Arg.Is<GetSlowEndpointsRequest>(r => r.Top == 2),
            Arg.Any<CancellationToken>());
    }

    private HttpClient CreateClient(
        IGetTrafficTrendsUseCase? trafficTrends = null,
        IGetStatusCodeDistributionUseCase? statusCodeDistribution = null,
        IGetLatencyAnalysisUseCase? latencyAnalysis = null,
        IGetSlowEndpointsUseCase? slowEndpoints = null)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Database=test");
            builder.ConfigureServices(services =>
            {
                if (trafficTrends is not null)
                    services.ReplaceService(trafficTrends);

                if (statusCodeDistribution is not null)
                    services.ReplaceService(statusCodeDistribution);

                if (latencyAnalysis is not null)
                    services.ReplaceService(latencyAnalysis);

                if (slowEndpoints is not null)
                    services.ReplaceService(slowEndpoints);
            });
        }).CreateClient();
    }
}
