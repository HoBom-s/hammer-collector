using FluentAssertions;
using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Application.UseCases.Search.SearchErrorLogs;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Ports;
using NSubstitute;

namespace Hammer.Collector.Tests.Application.UseCases.Search;

public sealed class SearchErrorLogsUseCaseTests
{
    private static readonly DateTimeOffset _from = new(2026, 3, 22, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _to = new(2026, 3, 23, 0, 0, 0, TimeSpan.Zero);

    private readonly ISearchRepository _searchRepository = Substitute.For<ISearchRepository>();
    private readonly SearchErrorLogsUseCase _sut;

    public SearchErrorLogsUseCaseTests()
    {
        _sut = new SearchErrorLogsUseCase(_searchRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResults_WhenValidRequestAsync()
    {
        // Arrange
        var expected = new ErrorListResult([], 0, 1, 20);
        _searchRepository
            .SearchErrorLogsAsync(
                _from,
                _to,
                null,
                null,
                null,
                1,
                20,
                Arg.Any<CancellationToken>())
            .Returns(expected);

        var request = new SearchErrorLogsRequest(_from, _to);

        // Act
        ErrorListResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassFilters_ToRepositoryAsync()
    {
        // Arrange
        _searchRepository
            .SearchErrorLogsAsync(
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(new ErrorListResult([], 0, 1, 10));

        var request = new SearchErrorLogsRequest(
            _from,
            _to,
            "trace-456",
            "NullReferenceException",
            "Hammer.User",
            1,
            10);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _searchRepository.Received(1).SearchErrorLogsAsync(
            _from,
            _to,
            "trace-456",
            "NullReferenceException",
            "Hammer.User",
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
        var request = new SearchErrorLogsRequest(_to, _from);
#pragma warning restore S2234
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenPageIsZeroAsync()
    {
        var request = new SearchErrorLogsRequest(_from, _to, Page: 0);
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenPageSizeIsZeroAsync()
    {
        var request = new SearchErrorLogsRequest(_from, _to, PageSize: 0);
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
