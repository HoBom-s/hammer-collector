using FluentAssertions;
using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Application.UseCases.Analytics.GetSlowEndpoints;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Ports;
using NSubstitute;

namespace Hammer.Collector.Tests.Application.UseCases.Analytics.GetSlowEndpoints;

public sealed class GetSlowEndpointsUseCaseTests
{
    private static readonly DateTimeOffset From = new(2026, 3, 22, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset To = new(2026, 3, 23, 0, 0, 0, TimeSpan.Zero);

    private readonly IAnalyticsRepository _analyticsRepository = Substitute.For<IAnalyticsRepository>();
    private readonly GetSlowEndpointsUseCase _sut;

    public GetSlowEndpointsUseCaseTests()
    {
        _sut = new GetSlowEndpointsUseCase(_analyticsRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEndpoints_WhenDataExistsAsync()
    {
        // Arrange
        var expected = new SlowEndpointResult([
            new SlowEndpointEntry("GET", "/api/users", 216.67, 500, 3),
            new SlowEndpointEntry("POST", "/api/users", 200.0, 200, 1),
        ]);
        _analyticsRepository
            .GetSlowEndpointsAsync(From, To, 10, Arg.Any<CancellationToken>())
            .Returns(expected);

        var request = new GetSlowEndpointsRequest(From, To);

        // Act
        SlowEndpointResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Endpoints.Should().HaveCount(2);
        result.Endpoints[0].Method.Should().Be("GET");
        result.Endpoints[1].Method.Should().Be("POST");
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
    public async Task ExecuteAsync_ShouldPassTopParameter_ToRepositoryAsync()
    {
        // Arrange
        _analyticsRepository
            .GetSlowEndpointsAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new SlowEndpointResult([]));

        var request = new GetSlowEndpointsRequest(From, To, 5);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _analyticsRepository.Received(1)
            .GetSlowEndpointsAsync(From, To, 5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyEndpoints_WhenNoDataExistsAsync()
    {
        // Arrange
        _analyticsRepository
            .GetSlowEndpointsAsync(From, To, 10, Arg.Any<CancellationToken>())
            .Returns(new SlowEndpointResult([]));

        var request = new GetSlowEndpointsRequest(From, To);

        // Act
        SlowEndpointResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Endpoints.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseDefaultTop_WhenNotSpecifiedAsync()
    {
        // Arrange
        _analyticsRepository
            .GetSlowEndpointsAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new SlowEndpointResult([]));

        var request = new GetSlowEndpointsRequest(From, To);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _analyticsRepository.Received(1)
            .GetSlowEndpointsAsync(From, To, 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenFromIsAfterToAsync()
    {
        // Arrange
#pragma warning disable S2234 // Arguments intentionally swapped to test validation
        var request = new GetSlowEndpointsRequest(To, From);
#pragma warning restore S2234

        // Act
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenTopIsZeroAsync()
    {
        // Arrange
        var request = new GetSlowEndpointsRequest(From, To, 0);

        // Act
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenTopIsNegativeAsync()
    {
        // Arrange
        var request = new GetSlowEndpointsRequest(From, To, -1);

        // Act
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
