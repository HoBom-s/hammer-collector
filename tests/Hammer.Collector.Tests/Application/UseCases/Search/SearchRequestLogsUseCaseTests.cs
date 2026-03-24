using FluentAssertions;
using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Application.UseCases.Search.SearchRequestLogs;
using Hammer.Collector.Domain.Ports;
using Hammer.Collector.Domain.Search;
using NSubstitute;

namespace Hammer.Collector.Tests.Application.UseCases.Search;

public sealed class SearchRequestLogsUseCaseTests
{
    private static readonly DateTimeOffset _from = new(2026, 3, 22, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _to = new(2026, 3, 23, 0, 0, 0, TimeSpan.Zero);

    private readonly ISearchRepository _searchRepository = Substitute.For<ISearchRepository>();
    private readonly SearchRequestLogsUseCase _sut;

    public SearchRequestLogsUseCaseTests()
    {
        _sut = new SearchRequestLogsUseCase(_searchRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResults_WhenValidRequestAsync()
    {
        // Arrange
        var expected = new RequestLogSearchResult([], 0, 1, 20);
        _searchRepository
            .SearchRequestLogsAsync(
                _from,
                _to,
                null,
                null,
                null,
                null,
                1,
                20,
                Arg.Any<CancellationToken>())
            .Returns(expected);

        var request = new SearchRequestLogsRequest(_from, _to);

        // Act
        RequestLogSearchResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassFilters_ToRepositoryAsync()
    {
        // Arrange
        _searchRepository
            .SearchRequestLogsAsync(
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(new RequestLogSearchResult([], 0, 1, 10));

        var request = new SearchRequestLogsRequest(
            _from,
            _to,
            "trace-123",
            500,
            "GET",
            "/api/users",
            1,
            10);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _searchRepository.Received(1).SearchRequestLogsAsync(
            _from,
            _to,
            "trace-123",
            500,
            "GET",
            "/api/users",
            1,
            10,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenRequestIsNullAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenFromIsAfterToAsync()
    {
#pragma warning disable S2234
        var request = new SearchRequestLogsRequest(_to, _from);
#pragma warning restore S2234
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenPageIsZeroAsync()
    {
        var request = new SearchRequestLogsRequest(_from, _to, Page: 0);
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenPageSizeIsZeroAsync()
    {
        var request = new SearchRequestLogsRequest(_from, _to, PageSize: 0);
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
