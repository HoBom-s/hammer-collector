using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hammer.Collector.Application.UseCases.Analytics.GetErrorDistribution;
using Hammer.Collector.Application.UseCases.Analytics.GetErrorTrend;
using Hammer.Collector.Application.UseCases.Analytics.GetRecentErrors;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Hammer.Collector.Tests.Api.Controllers;

public sealed class ErrorControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly DateTimeOffset _from = new(2026, 3, 22, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _to = new(2026, 3, 23, 0, 0, 0, TimeSpan.Zero);
    private static readonly string _fromEncoded = Uri.EscapeDataString(_from.ToString("O"));
    private static readonly string _toEncoded = Uri.EscapeDataString(_to.ToString("O"));

    private readonly WebApplicationFactory<Program> _factory;

    public ErrorControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTrend_ShouldReturn200WithPointsAsync()
    {
        // Arrange
        IGetErrorTrendUseCase useCase = Substitute.For<IGetErrorTrendUseCase>();
        useCase.ExecuteAsync(Arg.Any<GetErrorTrendRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ErrorTrendResult([
                new ErrorTrendPoint(_from, 5),
                new ErrorTrendPoint(_from.AddHours(1), 3),
            ]));

        HttpClient client = CreateClient(errorTrend: useCase);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri($"/analytics/errors/trend?from={_fromEncoded}&to={_toEncoded}", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ErrorTrendResult? body = await response.Content.ReadFromJsonAsync<ErrorTrendResult>();
        body!.Points.Should().HaveCount(2);
        body.Points[0].ErrorCount.Should().Be(5);
        body.Points[1].ErrorCount.Should().Be(3);
    }

    [Fact]
    public async Task GetDistribution_ShouldReturn200WithGroupingsAsync()
    {
        // Arrange
        IGetErrorDistributionUseCase useCase = Substitute.For<IGetErrorDistributionUseCase>();
        useCase.ExecuteAsync(Arg.Any<GetErrorDistributionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ErrorDistributionResult(
                [new ErrorDistributionEntry("NullReferenceException", 10)],
                [new ErrorDistributionEntry("Hammer.User", 8)],
                [new ErrorDistributionEntry("Error", 12)]));

        HttpClient client = CreateClient(errorDistribution: useCase);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri($"/analytics/errors/distribution?from={_fromEncoded}&to={_toEncoded}", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ErrorDistributionResult? body = await response.Content
            .ReadFromJsonAsync<ErrorDistributionResult>();
        body!.ByExceptionType.Should().HaveCount(1);
        body.ByExceptionType[0].Key.Should().Be("NullReferenceException");
        body.BySource[0].Key.Should().Be("Hammer.User");
        body.ByLevel[0].Key.Should().Be("Error");
    }

    [Fact]
    public async Task GetRecent_ShouldReturn200WithPaginatedResultsAsync()
    {
        // Arrange
        IGetRecentErrorsUseCase useCase = Substitute.For<IGetRecentErrorsUseCase>();
        useCase.ExecuteAsync(Arg.Any<GetRecentErrorsRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ErrorListResult(
                [
                    new ErrorDetail(
                        1,
                        "trace-1",
                        "Hammer.User",
                        "Error",
                        "NullReferenceException",
                        "Object reference not set",
                        null,
                        "/api/users",
                        "GET",
                        _from.AddHours(2)),
                    new ErrorDetail(
                        2,
                        "trace-2",
                        "Hammer.Order",
                        "Warning",
                        "InvalidOperationException",
                        "Invalid state",
                        null,
                        "/api/orders",
                        "POST",
                        _from.AddHours(1)),
                ],
                10,
                1,
                2));

        HttpClient client = CreateClient(recentErrors: useCase);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri($"/analytics/errors/recent?from={_fromEncoded}&to={_toEncoded}&page=1&pageSize=2", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ErrorListResult? body = await response.Content.ReadFromJsonAsync<ErrorListResult>();
        body!.Errors.Should().HaveCount(2);
        body.TotalCount.Should().Be(10);
        body.Page.Should().Be(1);
        body.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetRecent_ShouldPassPaginationParametersAsync()
    {
        // Arrange
        IGetRecentErrorsUseCase useCase = Substitute.For<IGetRecentErrorsUseCase>();
        useCase.ExecuteAsync(Arg.Any<GetRecentErrorsRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ErrorListResult([], 0, 2, 5));

        HttpClient client = CreateClient(recentErrors: useCase);

        // Act
        await client.GetAsync(
            new Uri($"/analytics/errors/recent?from={_fromEncoded}&to={_toEncoded}&page=2&pageSize=5", UriKind.Relative));

        // Assert
        await useCase.Received(1).ExecuteAsync(
            Arg.Is<GetRecentErrorsRequest>(r => r.Page == 2 && r.PageSize == 5),
            Arg.Any<CancellationToken>());
    }

    private HttpClient CreateClient(
        IGetErrorTrendUseCase? errorTrend = null,
        IGetErrorDistributionUseCase? errorDistribution = null,
        IGetRecentErrorsUseCase? recentErrors = null)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Database=test");
            builder.ConfigureServices(services =>
            {
                if (errorTrend is not null)
                    services.ReplaceService(errorTrend);

                if (errorDistribution is not null)
                    services.ReplaceService(errorDistribution);

                if (recentErrors is not null)
                    services.ReplaceService(recentErrors);
            });
        }).CreateClient();
    }
}
