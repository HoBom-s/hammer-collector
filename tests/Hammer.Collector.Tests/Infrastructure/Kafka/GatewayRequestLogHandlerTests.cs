using System.Text.Json;
using FluentAssertions;
using Hammer.Collector.Domain.Entities;
using Hammer.Collector.Infrastructure.Kafka;
using Hammer.Collector.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Collector.Tests.Infrastructure.Kafka;

public sealed class GatewayRequestLogHandlerTests : IDisposable
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly CollectorDbContext _db;
    private readonly GatewayRequestLogHandler _sut = new();

    public GatewayRequestLogHandlerTests()
    {
        DbContextOptions<CollectorDbContext> options = new DbContextOptionsBuilder<CollectorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new CollectorDbContext(options);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public void Topic_ShouldReturnGatewayRequestLogTopic()
    {
        _sut.Topic.Should().Be("gateway-request-log");
    }

    [Fact]
    public void Handle_ShouldAddEntities_WhenValidMessagesAsync()
    {
        // Arrange
        GatewayRequestLog log1 = new()
        {
            TraceId = "trace-1",
            Method = "GET",
            Path = "/api/users",
            StatusCode = 200,
            DurationMs = 50,
            Timestamp = DateTimeOffset.UtcNow,
        };

        GatewayRequestLog log2 = new()
        {
            TraceId = "trace-2",
            Method = "POST",
            Path = "/api/orders",
            StatusCode = 201,
            DurationMs = 120,
            Timestamp = DateTimeOffset.UtcNow,
        };

        List<string> messages =
        [
            JsonSerializer.Serialize(log1, _jsonOptions),
            JsonSerializer.Serialize(log2, _jsonOptions),
        ];

        // Act
        _sut.Handle(messages, _db);

        // Assert
        _db.ChangeTracker.Entries<GatewayRequestLog>().Should().HaveCount(2);
    }

    [Fact]
    public void Handle_ShouldSkipNullEntries_WhenInvalidJsonAsync()
    {
        // Arrange
        GatewayRequestLog validLog = new()
        {
            TraceId = "trace-1",
            Method = "GET",
            Path = "/api/users",
            StatusCode = 200,
            DurationMs = 50,
            Timestamp = DateTimeOffset.UtcNow,
        };

        List<string> messages =
        [
            JsonSerializer.Serialize(validLog, _jsonOptions),
            "null",
        ];

        // Act
        _sut.Handle(messages, _db);

        // Assert
        _db.ChangeTracker.Entries<GatewayRequestLog>().Should().HaveCount(1);
    }

    [Fact]
    public void Handle_ShouldHandleEmptyList()
    {
        // Act
        _sut.Handle([], _db);

        // Assert
        _db.ChangeTracker.Entries<GatewayRequestLog>().Should().BeEmpty();
    }
}
