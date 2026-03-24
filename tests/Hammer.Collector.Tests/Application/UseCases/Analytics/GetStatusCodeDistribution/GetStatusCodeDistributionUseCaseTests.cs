using FluentAssertions;
using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Application.UseCases.Analytics.GetStatusCodeDistribution;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Enums;
using Hammer.Collector.Domain.Ports;
using NSubstitute;

namespace Hammer.Collector.Tests.Application.UseCases.Analytics.GetStatusCodeDistribution;

public sealed class GetStatusCodeDistributionUseCaseTests
{
    private static readonly DateTimeOffset From = new(2026, 3, 22, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset To = new(2026, 3, 23, 0, 0, 0, TimeSpan.Zero);

    private readonly IAnalyticsRepository _analyticsRepository = Substitute.For<IAnalyticsRepository>();
    private readonly GetStatusCodeDistributionUseCase _sut;

    public GetStatusCodeDistributionUseCaseTests()
    {
        _sut = new GetStatusCodeDistributionUseCase(_analyticsRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnDistribution_WhenDataExistsAsync()
    {
        // Arrange
        var expected = new StatusCodeDistributionResult(
            [new StatusCodeSummary(2, 80, 80.0), new StatusCodeSummary(5, 20, 20.0)],
            [new StatusCodeBucket(From, 2, 80)]);
        _analyticsRepository
            .GetStatusCodeDistributionAsync(From, To, TimeBucket.Hour, Arg.Any<CancellationToken>())
            .Returns(expected);

        var request = new GetStatusCodeDistributionRequest(From, To, TimeBucket.Hour);

        // Act
        StatusCodeDistributionResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Summary.Should().HaveCount(2);
        result.Summary[0].TotalCount.Should().Be(80);
        result.Summary[1].TotalCount.Should().Be(20);
        result.TimeSeries.Should().HaveCount(1);
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
            .GetStatusCodeDistributionAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<TimeBucket>(), Arg.Any<CancellationToken>())
            .Returns(new StatusCodeDistributionResult([], []));

        var request = new GetStatusCodeDistributionRequest(From, To, TimeBucket.Day);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _analyticsRepository.Received(1)
            .GetStatusCodeDistributionAsync(From, To, TimeBucket.Day, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyResult_WhenNoDataExistsAsync()
    {
        // Arrange
        _analyticsRepository
            .GetStatusCodeDistributionAsync(From, To, TimeBucket.Hour, Arg.Any<CancellationToken>())
            .Returns(new StatusCodeDistributionResult([], []));

        var request = new GetStatusCodeDistributionRequest(From, To);

        // Act
        StatusCodeDistributionResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Summary.Should().BeEmpty();
        result.TimeSeries.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenFromIsAfterToAsync()
    {
        // Arrange
#pragma warning disable S2234 // Arguments intentionally swapped to test validation
        var request = new GetStatusCodeDistributionRequest(To, From);
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
        var request = new GetStatusCodeDistributionRequest(From, From);

        // Act
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
