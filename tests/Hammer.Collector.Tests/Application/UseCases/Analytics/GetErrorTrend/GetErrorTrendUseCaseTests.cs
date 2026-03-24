using FluentAssertions;
using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Application.UseCases.Analytics.GetErrorTrend;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Enums;
using Hammer.Collector.Domain.Ports;
using NSubstitute;

namespace Hammer.Collector.Tests.Application.UseCases.Analytics.GetErrorTrend;

public sealed class GetErrorTrendUseCaseTests
{
    private static readonly DateTimeOffset _from = new(2026, 3, 22, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _to = new(2026, 3, 23, 0, 0, 0, TimeSpan.Zero);

    private readonly IAnalyticsRepository _analyticsRepository = Substitute.For<IAnalyticsRepository>();
    private readonly GetErrorTrendUseCase _sut;

    public GetErrorTrendUseCaseTests()
    {
        _sut = new GetErrorTrendUseCase(_analyticsRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTrend_WhenDataExistsAsync()
    {
        // Arrange
        var expected = new ErrorTrendResult([
            new ErrorTrendPoint(_from, 5),
            new ErrorTrendPoint(_from.AddHours(1), 3),
        ]);
        _analyticsRepository
            .GetErrorTrendAsync(_from, _to, TimeBucket.Hour, Arg.Any<CancellationToken>())
            .Returns(expected);

        var request = new GetErrorTrendRequest(_from, _to, TimeBucket.Hour);

        // Act
        ErrorTrendResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Points.Should().HaveCount(2);
        result.Points[0].ErrorCount.Should().Be(5);
        result.Points[1].ErrorCount.Should().Be(3);
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
            .GetErrorTrendAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<TimeBucket>(), Arg.Any<CancellationToken>())
            .Returns(new ErrorTrendResult([]));

        var request = new GetErrorTrendRequest(_from, _to, TimeBucket.Day);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _analyticsRepository.Received(1)
            .GetErrorTrendAsync(_from, _to, TimeBucket.Day, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyPoints_WhenNoDataExistsAsync()
    {
        // Arrange
        _analyticsRepository
            .GetErrorTrendAsync(_from, _to, TimeBucket.Hour, Arg.Any<CancellationToken>())
            .Returns(new ErrorTrendResult([]));

        var request = new GetErrorTrendRequest(_from, _to);

        // Act
        ErrorTrendResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Points.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenFromIsAfterToAsync()
    {
        // Arrange
#pragma warning disable S2234 // Arguments intentionally swapped to test validation
        var request = new GetErrorTrendRequest(_to, _from);
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
        var request = new GetErrorTrendRequest(_from, _from);

        // Act
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
