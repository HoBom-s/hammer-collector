using FluentAssertions;
using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Application.UseCases.Analytics.GetLatencyAnalysis;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Ports;
using NSubstitute;

namespace Hammer.Collector.Tests.Application.UseCases.Analytics.GetLatencyAnalysis;

public sealed class GetLatencyAnalysisUseCaseTests
{
    private static readonly DateTimeOffset _from = new(2026, 3, 22, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _to = new(2026, 3, 23, 0, 0, 0, TimeSpan.Zero);

    private readonly IAnalyticsRepository _analyticsRepository = Substitute.For<IAnalyticsRepository>();
    private readonly GetLatencyAnalysisUseCase _sut;

    public GetLatencyAnalysisUseCaseTests()
    {
        _sut = new GetLatencyAnalysisUseCase(_analyticsRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPercentiles_WhenDataExistsAsync()
    {
        // Arrange
        var expected = new LatencyAnalysisResult(176.0, 500, 100.0, 440.0, 488.0, 5);
        _analyticsRepository
            .GetLatencyAnalysisAsync(_from, _to, Arg.Any<CancellationToken>())
            .Returns(expected);

        var request = new GetLatencyAnalysisRequest(_from, _to);

        // Act
        LatencyAnalysisResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.P50Ms.Should().Be(100.0);
        result.P95Ms.Should().Be(440.0);
        result.MaxMs.Should().Be(500);
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
            .GetLatencyAnalysisAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new LatencyAnalysisResult(0, 0, 0, 0, 0, 0));

        var request = new GetLatencyAnalysisRequest(_from, _to);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _analyticsRepository.Received(1)
            .GetLatencyAnalysisAsync(_from, _to, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnZeros_WhenNoDataExistsAsync()
    {
        // Arrange
        _analyticsRepository
            .GetLatencyAnalysisAsync(_from, _to, Arg.Any<CancellationToken>())
            .Returns(new LatencyAnalysisResult(0, 0, 0, 0, 0, 0));

        var request = new GetLatencyAnalysisRequest(_from, _to);

        // Act
        LatencyAnalysisResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.AvgMs.Should().Be(0);
        result.MaxMs.Should().Be(0);
        result.P50Ms.Should().Be(0);
        result.P95Ms.Should().Be(0);
        result.P99Ms.Should().Be(0);
        result.TotalRequests.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenFromIsAfterToAsync()
    {
        // Arrange
#pragma warning disable S2234 // Arguments intentionally swapped to test validation
        var request = new GetLatencyAnalysisRequest(_to, _from);
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
        var request = new GetLatencyAnalysisRequest(_from, _from);

        // Act
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
