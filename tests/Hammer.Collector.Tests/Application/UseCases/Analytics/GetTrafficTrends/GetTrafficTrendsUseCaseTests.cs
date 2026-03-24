using FluentAssertions;
using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Application.UseCases.Analytics.GetTrafficTrends;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Enums;
using Hammer.Collector.Domain.Ports;
using NSubstitute;

namespace Hammer.Collector.Tests.Application.UseCases.Analytics.GetTrafficTrends;

public sealed class GetTrafficTrendsUseCaseTests
{
    private static readonly DateTimeOffset From = new(2026, 3, 22, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset To = new(2026, 3, 23, 0, 0, 0, TimeSpan.Zero);

    private readonly IAnalyticsRepository _analyticsRepository = Substitute.For<IAnalyticsRepository>();
    private readonly GetTrafficTrendsUseCase _sut;

    public GetTrafficTrendsUseCaseTests()
    {
        _sut = new GetTrafficTrendsUseCase(_analyticsRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTrend_WhenDataExistsAsync()
    {
        // Arrange
        var expected = new TrafficTrendResult([
            new TrafficTrendPoint(From, 100, 0.03),
            new TrafficTrendPoint(From.AddHours(1), 200, 0.06),
        ]);
        _analyticsRepository
            .GetTrafficTrendsAsync(From, To, TimeBucket.Hour, Arg.Any<CancellationToken>())
            .Returns(expected);

        var request = new GetTrafficTrendsRequest(From, To, TimeBucket.Hour);

        // Act
        TrafficTrendResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Points.Should().HaveCount(2);
        result.Points[0].RequestCount.Should().Be(100);
        result.Points[1].RequestCount.Should().Be(200);
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
    public async Task ExecuteAsync_ShouldCallRepository_WithCorrectParametersAsync()
    {
        // Arrange
        _analyticsRepository
            .GetTrafficTrendsAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<TimeBucket>(), Arg.Any<CancellationToken>())
            .Returns(new TrafficTrendResult([]));

        var request = new GetTrafficTrendsRequest(From, To, TimeBucket.Day);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _analyticsRepository.Received(1)
            .GetTrafficTrendsAsync(From, To, TimeBucket.Day, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyPoints_WhenNoDataExistsAsync()
    {
        // Arrange
        _analyticsRepository
            .GetTrafficTrendsAsync(From, To, TimeBucket.Hour, Arg.Any<CancellationToken>())
            .Returns(new TrafficTrendResult([]));

        var request = new GetTrafficTrendsRequest(From, To);

        // Act
        TrafficTrendResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Points.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseDefaultBucket_WhenNotSpecifiedAsync()
    {
        // Arrange
        _analyticsRepository
            .GetTrafficTrendsAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<TimeBucket>(), Arg.Any<CancellationToken>())
            .Returns(new TrafficTrendResult([]));

        var request = new GetTrafficTrendsRequest(From, To);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _analyticsRepository.Received(1)
            .GetTrafficTrendsAsync(From, To, TimeBucket.Hour, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenFromIsAfterToAsync()
    {
        // Arrange
#pragma warning disable S2234 // Arguments intentionally swapped to test validation
        var request = new GetTrafficTrendsRequest(To, From);
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
        var request = new GetTrafficTrendsRequest(From, From);

        // Act
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
