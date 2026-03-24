using FluentAssertions;
using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Application.UseCases.Analytics.GetErrorDistribution;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Ports;
using NSubstitute;

namespace Hammer.Collector.Tests.Application.UseCases.Analytics.GetErrorDistribution;

public sealed class GetErrorDistributionUseCaseTests
{
    private static readonly DateTimeOffset _from = new(2026, 3, 22, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _to = new(2026, 3, 23, 0, 0, 0, TimeSpan.Zero);

    private readonly IAnalyticsRepository _analyticsRepository = Substitute.For<IAnalyticsRepository>();
    private readonly GetErrorDistributionUseCase _sut;

    public GetErrorDistributionUseCaseTests()
    {
        _sut = new GetErrorDistributionUseCase(_analyticsRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnDistribution_WhenDataExistsAsync()
    {
        // Arrange
        var expected = new ErrorDistributionResult(
            [new ErrorDistributionEntry("NullReferenceException", 10)],
            [new ErrorDistributionEntry("Hammer.User", 8)],
            [new ErrorDistributionEntry("Error", 12)]);
        _analyticsRepository
            .GetErrorDistributionAsync(_from, _to, Arg.Any<CancellationToken>())
            .Returns(expected);

        var request = new GetErrorDistributionRequest(_from, _to);

        // Act
        ErrorDistributionResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.ByExceptionType.Should().HaveCount(1);
        result.ByExceptionType[0].Count.Should().Be(10);
        result.BySource.Should().HaveCount(1);
        result.BySource[0].Count.Should().Be(8);
        result.ByLevel.Should().HaveCount(1);
        result.ByLevel[0].Count.Should().Be(12);
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
    public async Task ExecuteAsync_ShouldCallRepository_WithCorrectDateRangeAsync()
    {
        // Arrange
        _analyticsRepository
            .GetErrorDistributionAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new ErrorDistributionResult([], [], []));

        var request = new GetErrorDistributionRequest(_from, _to);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _analyticsRepository.Received(1)
            .GetErrorDistributionAsync(_from, _to, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyDistributions_WhenNoDataExistsAsync()
    {
        // Arrange
        _analyticsRepository
            .GetErrorDistributionAsync(_from, _to, Arg.Any<CancellationToken>())
            .Returns(new ErrorDistributionResult([], [], []));

        var request = new GetErrorDistributionRequest(_from, _to);

        // Act
        ErrorDistributionResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.ByExceptionType.Should().BeEmpty();
        result.BySource.Should().BeEmpty();
        result.ByLevel.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenFromIsAfterToAsync()
    {
        // Arrange
#pragma warning disable S2234 // Arguments intentionally swapped to test validation
        var request = new GetErrorDistributionRequest(_to, _from);
#pragma warning restore S2234

        // Act
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenFromEqualsToAsync()
    {
        // Arrange
        var request = new GetErrorDistributionRequest(_from, _from);

        // Act
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
