using FluentAssertions;
using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Application.UseCases.Analytics.GetRecentErrors;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Ports;
using NSubstitute;

namespace Hammer.Collector.Tests.Application.UseCases.Analytics.GetRecentErrors;

public sealed class GetRecentErrorsUseCaseTests
{
    private static readonly DateTimeOffset _from = new(2026, 3, 22, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _to = new(2026, 3, 23, 0, 0, 0, TimeSpan.Zero);

    private readonly IAnalyticsRepository _analyticsRepository = Substitute.For<IAnalyticsRepository>();
    private readonly GetRecentErrorsUseCase _sut;

    public GetRecentErrorsUseCaseTests()
    {
        _sut = new GetRecentErrorsUseCase(_analyticsRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPaginatedResults_WhenDataExistsAsync()
    {
        // Arrange
        var expected = new ErrorListResult(
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
            ],
            10,
            1,
            20);
        _analyticsRepository
            .GetRecentErrorsAsync(_from, _to, 1, 20, Arg.Any<CancellationToken>())
            .Returns(expected);

        var request = new GetRecentErrorsRequest(_from, _to);

        // Act
        ErrorListResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Errors.Should().HaveCount(1);
        result.TotalCount.Should().Be(10);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenRequestIsNullAsync()
    {
        // Act
        Func<Task> act = () => _sut.ExecuteAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassPaginationParameters_ToRepositoryAsync()
    {
        // Arrange
        _analyticsRepository
            .GetRecentErrorsAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new ErrorListResult([], 0, 2, 5));

        var request = new GetRecentErrorsRequest(_from, _to, 2, 5);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _analyticsRepository.Received(1)
            .GetRecentErrorsAsync(_from, _to, 2, 5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyErrors_WhenNoDataExistsAsync()
    {
        // Arrange
        _analyticsRepository
            .GetRecentErrorsAsync(_from, _to, 1, 20, Arg.Any<CancellationToken>())
            .Returns(new ErrorListResult([], 0, 1, 20));

        var request = new GetRecentErrorsRequest(_from, _to);

        // Act
        ErrorListResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Errors.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseDefaultPagination_WhenNotSpecifiedAsync()
    {
        // Arrange
        _analyticsRepository
            .GetRecentErrorsAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new ErrorListResult([], 0, 1, 20));

        var request = new GetRecentErrorsRequest(_from, _to);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _analyticsRepository.Received(1)
            .GetRecentErrorsAsync(_from, _to, 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenFromIsAfterToAsync()
    {
        // Arrange
#pragma warning disable S2234 // Arguments intentionally swapped to test validation
        var request = new GetRecentErrorsRequest(_to, _from);
#pragma warning restore S2234

        // Act
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenPageIsZeroAsync()
    {
        // Arrange
        var request = new GetRecentErrorsRequest(_from, _to, 0);

        // Act
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenPageSizeIsZeroAsync()
    {
        // Arrange
        var request = new GetRecentErrorsRequest(_from, _to, 1, 0);

        // Act
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenPageSizeIsNegativeAsync()
    {
        // Arrange
        var request = new GetRecentErrorsRequest(_from, _to, 1, -5);

        // Act
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
