using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hammer.Collector.Application.UseCases.Search.SearchByTraceId;
using Hammer.Collector.Application.UseCases.Search.SearchErrorLogs;
using Hammer.Collector.Application.UseCases.Search.SearchRequestLogs;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Search;
using Hammer.Collector.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Hammer.Collector.Tests.Api.Controllers;

public sealed class SearchControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly DateTimeOffset _from = new(2026, 3, 22, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _to = new(2026, 3, 23, 0, 0, 0, TimeSpan.Zero);
    private static readonly string _fromEncoded = Uri.EscapeDataString(_from.ToString("O"));
    private static readonly string _toEncoded = Uri.EscapeDataString(_to.ToString("O"));

    private readonly WebApplicationFactory<Program> _factory;

    public SearchControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SearchRequests_ShouldReturn200WithPaginatedResultsAsync()
    {
        // Arrange
        ISearchRequestLogsUseCase useCase = Substitute.For<ISearchRequestLogsUseCase>();
        useCase.ExecuteAsync(Arg.Any<SearchRequestLogsRequest>(), Arg.Any<CancellationToken>())
            .Returns(new RequestLogSearchResult(
                [
                    new RequestLogDetail(
                        1,
                        "trace-1",
                        null,
                        "GET",
                        "/api/users",
                        null,
                        200,
                        50,
                        "127.0.0.1",
                        null,
                        null,
                        _from),
                ],
                1,
                1,
                20));

        HttpClient client = CreateClient(searchRequestLogs: useCase);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri(
                $"/search/requests?from={_fromEncoded}&to={_toEncoded}",
                UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RequestLogSearchResult? body = await response.Content
            .ReadFromJsonAsync<RequestLogSearchResult>();
        body!.Logs.Should().HaveCount(1);
        body.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchRequests_ShouldPassFiltersAsync()
    {
        // Arrange
        ISearchRequestLogsUseCase useCase = Substitute.For<ISearchRequestLogsUseCase>();
        useCase.ExecuteAsync(Arg.Any<SearchRequestLogsRequest>(), Arg.Any<CancellationToken>())
            .Returns(new RequestLogSearchResult([], 0, 1, 20));

        HttpClient client = CreateClient(searchRequestLogs: useCase);

        // Act
        await client.GetAsync(
            new Uri(
                $"/search/requests?from={_fromEncoded}&to={_toEncoded}&statusCode=500&method=GET",
                UriKind.Relative));

        // Assert
        await useCase.Received(1).ExecuteAsync(
            Arg.Is<SearchRequestLogsRequest>(r =>
                r.StatusCode == 500 && r.Method == "GET"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchErrors_ShouldReturn200WithResultsAsync()
    {
        // Arrange
        ISearchErrorLogsUseCase useCase = Substitute.For<ISearchErrorLogsUseCase>();
        useCase.ExecuteAsync(Arg.Any<SearchErrorLogsRequest>(), Arg.Any<CancellationToken>())
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
                        _from),
                ],
                1,
                1,
                20));

        HttpClient client = CreateClient(searchErrorLogs: useCase);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri(
                $"/search/errors?from={_fromEncoded}&to={_toEncoded}&source=Hammer.User",
                UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ErrorListResult? body = await response.Content.ReadFromJsonAsync<ErrorListResult>();
        body!.Errors.Should().HaveCount(1);
        body.Errors[0].Source.Should().Be("Hammer.User");
    }

    [Fact]
    public async Task SearchByTraceId_ShouldReturn200WithCombinedResultsAsync()
    {
        // Arrange
        ISearchByTraceIdUseCase useCase = Substitute.For<ISearchByTraceIdUseCase>();
        useCase.ExecuteAsync(Arg.Any<SearchByTraceIdRequest>(), Arg.Any<CancellationToken>())
            .Returns(new TraceSearchResult(
                [
                    new RequestLogDetail(
                        1,
                        "trace-abc",
                        null,
                        "GET",
                        "/api/users",
                        null,
                        500,
                        100,
                        null,
                        null,
                        null,
                        _from),
                ],
                [
                    new ErrorDetail(
                        1,
                        "trace-abc",
                        "Hammer.User",
                        "Error",
                        "NullReferenceException",
                        "Object reference not set",
                        null,
                        "/api/users",
                        "GET",
                        _from),
                ]));

        HttpClient client = CreateClient(searchByTraceId: useCase);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            new Uri("/search/trace/trace-abc", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TraceSearchResult? body = await response.Content.ReadFromJsonAsync<TraceSearchResult>();
        body!.RequestLogs.Should().HaveCount(1);
        body.ErrorLogs.Should().HaveCount(1);
        body.RequestLogs[0].TraceId.Should().Be("trace-abc");
        body.ErrorLogs[0].TraceId.Should().Be("trace-abc");
    }

    private HttpClient CreateClient(
        ISearchRequestLogsUseCase? searchRequestLogs = null,
        ISearchErrorLogsUseCase? searchErrorLogs = null,
        ISearchByTraceIdUseCase? searchByTraceId = null)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Database=test");
            builder.ConfigureServices(services =>
            {
                if (searchRequestLogs is not null)
                    services.ReplaceService(searchRequestLogs);

                if (searchErrorLogs is not null)
                    services.ReplaceService(searchErrorLogs);

                if (searchByTraceId is not null)
                    services.ReplaceService(searchByTraceId);
            });
        }).CreateClient();
    }
}
