using FluentAssertions;
using Hammer.Collector.Domain.Entities;
using Hammer.Collector.Infrastructure.Persistence;
using Hammer.Collector.Tests.Fixtures;

namespace Hammer.Collector.Tests.Infrastructure.Persistence;

public sealed class CollectorDbContextTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public CollectorDbContextTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ShouldInsertAndRetrieveGatewayRequestLogAsync()
    {
        // Arrange
        await using CollectorDbContext context = _fixture.CreateDbContext();
        GatewayRequestLog log = new()
        {
            TraceId = "trace-001",
            Method = "GET",
            Path = "/api/users",
            StatusCode = 200,
            DurationMs = 42,
            ClientIp = "127.0.0.1",
            UserAgent = "TestAgent/1.0",
            RouteCluster = "user-cluster",
            Timestamp = DateTimeOffset.UtcNow,
        };

        // Act
        context.GatewayRequestLogs.Add(log);
        await context.SaveChangesAsync();

        // Assert
        GatewayRequestLog? saved = await context.GatewayRequestLogs
            .FindAsync(log.Id);

        saved.Should().NotBeNull();
        saved!.TraceId.Should().Be("trace-001");
        saved.Method.Should().Be("GET");
        saved.Path.Should().Be("/api/users");
        saved.StatusCode.Should().Be(200);
        saved.DurationMs.Should().Be(42);
        saved.ClientIp.Should().Be("127.0.0.1");
        saved.RouteCluster.Should().Be("user-cluster");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ShouldInsertAndRetrieveServiceErrorLogAsync()
    {
        // Arrange
        await using CollectorDbContext context = _fixture.CreateDbContext();
        ServiceErrorLog log = new()
        {
            TraceId = "trace-err-001",
            Source = "Hammer.User",
            Level = "Error",
            ExceptionType = "System.InvalidOperationException",
            Message = "Something went wrong",
            StackTrace = "at Hammer.User.SomeMethod()",
            RequestPath = "/api/users/1",
            RequestMethod = "POST",
            Timestamp = DateTimeOffset.UtcNow,
        };

        // Act
        context.ServiceErrorLogs.Add(log);
        await context.SaveChangesAsync();

        // Assert
        ServiceErrorLog? saved = await context.ServiceErrorLogs
            .FindAsync(log.Id);

        saved.Should().NotBeNull();
        saved!.TraceId.Should().Be("trace-err-001");
        saved.Source.Should().Be("Hammer.User");
        saved.ExceptionType.Should().Be("System.InvalidOperationException");
        saved.Message.Should().Be("Something went wrong");
        saved.StackTrace.Should().Be("at Hammer.User.SomeMethod()");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ShouldAutoIncrementIdAsync()
    {
        // Arrange
        await using CollectorDbContext context = _fixture.CreateDbContext();
        GatewayRequestLog log1 = new()
        {
            TraceId = "trace-inc-1",
            Method = "GET",
            Path = "/a",
            StatusCode = 200,
            DurationMs = 1,
            Timestamp = DateTimeOffset.UtcNow,
        };

        GatewayRequestLog log2 = new()
        {
            TraceId = "trace-inc-2",
            Method = "POST",
            Path = "/b",
            StatusCode = 201,
            DurationMs = 2,
            Timestamp = DateTimeOffset.UtcNow,
        };

        // Act
        context.GatewayRequestLogs.Add(log1);
        context.GatewayRequestLogs.Add(log2);
        await context.SaveChangesAsync();

        // Assert
        log1.Id.Should().BeGreaterThan(0);
        log2.Id.Should().BeGreaterThan(log1.Id);
    }
}
