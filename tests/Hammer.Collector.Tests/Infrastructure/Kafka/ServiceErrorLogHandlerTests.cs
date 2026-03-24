using System.Text.Json;
using FluentAssertions;
using Hammer.Collector.Domain.Entities;
using Hammer.Collector.Infrastructure.Kafka;
using Hammer.Collector.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Collector.Tests.Infrastructure.Kafka;

public sealed class ServiceErrorLogHandlerTests : IDisposable
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly CollectorDbContext _db;
    private readonly ServiceErrorLogHandler _sut = new();

    public ServiceErrorLogHandlerTests()
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
    public void Topic_ShouldReturnServiceErrorLogTopic()
    {
        _sut.Topic.Should().Be("service-error-log");
    }

    [Fact]
    public void Handle_ShouldAddEntities_WhenValidMessagesAsync()
    {
        // Arrange
        ServiceErrorLog log1 = new()
        {
            TraceId = "trace-1",
            Source = "Hammer.User",
            Level = "Error",
            ExceptionType = "NullReferenceException",
            Message = "Object reference not set",
            RequestPath = "/api/users",
            RequestMethod = "GET",
            Timestamp = DateTimeOffset.UtcNow,
        };

        ServiceErrorLog log2 = new()
        {
            TraceId = "trace-2",
            Source = "Hammer.Order",
            Level = "Warning",
            ExceptionType = "InvalidOperationException",
            Message = "Invalid state",
            RequestPath = "/api/orders",
            RequestMethod = "POST",
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
        _db.ChangeTracker.Entries<ServiceErrorLog>().Should().HaveCount(2);
    }

    [Fact]
    public void Handle_ShouldSkipNullEntries_WhenInvalidJsonAsync()
    {
        // Arrange
        ServiceErrorLog validLog = new()
        {
            TraceId = "trace-1",
            Source = "Hammer.User",
            Level = "Error",
            ExceptionType = "NullReferenceException",
            Message = "Object reference not set",
            RequestPath = "/api/users",
            RequestMethod = "GET",
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
        _db.ChangeTracker.Entries<ServiceErrorLog>().Should().HaveCount(1);
    }

    [Fact]
    public void Handle_ShouldHandleEmptyList()
    {
        // Act
        _sut.Handle([], _db);

        // Assert
        _db.ChangeTracker.Entries<ServiceErrorLog>().Should().BeEmpty();
    }
}
