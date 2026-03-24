using FluentAssertions;
using Hammer.Collector.Api.Middleware;
using Hammer.Collector.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Hammer.Collector.Tests.Api.Middleware;

#pragma warning disable CA1873 // Test code verifies mock ILogger.Log calls directly
public sealed class ApplicationExceptionHandlerTests
{
    private readonly ILogger<ApplicationExceptionHandler> _logger =
        Substitute.For<ILogger<ApplicationExceptionHandler>>();

    private readonly ApplicationExceptionHandler _sut;

    public ApplicationExceptionHandlerTests()
    {
        _sut = new ApplicationExceptionHandler(_logger);
    }

    [Theory]
    [InlineData(typeof(BadRequestException), 400)]
    [InlineData(typeof(UnauthorizedException), 401)]
    [InlineData(typeof(ForbiddenException), 403)]
    [InlineData(typeof(NotFoundException), 404)]
    [InlineData(typeof(ConflictException), 409)]
    [InlineData(typeof(ServiceUnavailableException), 503)]
    public async Task TryHandleAsync_ShouldReturnTrue_WhenKnownExceptionAsync(Type exceptionType, int expectedStatusCode)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "test message")!;
        DefaultHttpContext httpContext = new() { Response = { Body = new MemoryStream() } };

        // Act
        var result = await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturnFalse_WhenUnknownExceptionAsync()
    {
        // Arrange
        var exception = new InvalidOperationException("unexpected");
        DefaultHttpContext httpContext = new();

        // Act
        var result = await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryHandleAsync_ShouldLogError_WhenUnknownExceptionAsync()
    {
        // Arrange
        var exception = new InvalidOperationException("unexpected");
        DefaultHttpContext httpContext = new() { Request = { Method = "GET", Path = "/api/analytics/traffic/trends" } };

        // Act
        await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task TryHandleAsync_ShouldNotLog_WhenKnownExceptionAsync()
    {
        // Arrange
        var exception = new NotFoundException("not found");
        DefaultHttpContext httpContext = new() { Response = { Body = new MemoryStream() } };

        // Act
        await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        _logger.DidNotReceive().Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
#pragma warning restore CA1873
