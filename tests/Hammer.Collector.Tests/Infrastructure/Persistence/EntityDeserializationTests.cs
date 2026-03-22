using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using Hammer.Collector.Domain.Entities;

namespace Hammer.Collector.Tests.Infrastructure.Persistence;

public sealed class EntityDeserializationTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public void ShouldDeserializeGatewayRequestLog()
    {
        // Arrange
        var json = """
        {
            "traceId": "abc-123",
            "userId": "user-1",
            "method": "GET",
            "path": "/api/auctions",
            "queryString": "?page=1",
            "statusCode": 200,
            "durationMs": 150,
            "clientIp": "10.0.0.1",
            "userAgent": "Mozilla/5.0",
            "requestSize": 0,
            "responseSize": 4096,
            "routeCluster": "auction-cluster",
            "timestamp": "2026-03-22T12:00:00+09:00"
        }
        """;

        // Act
        GatewayRequestLog? result = JsonSerializer.Deserialize<GatewayRequestLog>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.TraceId.Should().Be("abc-123");
        result.UserId.Should().Be("user-1");
        result.Method.Should().Be("GET");
        result.Path.Should().Be("/api/auctions");
        result.QueryString.Should().Be("?page=1");
        result.StatusCode.Should().Be(200);
        result.DurationMs.Should().Be(150);
        result.ClientIp.Should().Be("10.0.0.1");
        result.UserAgent.Should().Be("Mozilla/5.0");
        result.RequestSize.Should().Be(0);
        result.ResponseSize.Should().Be(4096);
        result.RouteCluster.Should().Be("auction-cluster");
        result.Timestamp.Should().Be(DateTimeOffset.Parse("2026-03-22T12:00:00+09:00", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ShouldDeserializeServiceErrorLog()
    {
        // Arrange
        var json = """
        {
            "traceId": "err-456",
            "source": "Hammer.Auction",
            "level": "Error",
            "exceptionType": "System.NullReferenceException",
            "message": "Object reference not set",
            "stackTrace": "at Hammer.Auction.Controllers.AuctionController.Get()",
            "requestPath": "/api/auctions/1",
            "requestMethod": "GET",
            "timestamp": "2026-03-22T13:00:00+09:00"
        }
        """;

        // Act
        ServiceErrorLog? result = JsonSerializer.Deserialize<ServiceErrorLog>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.TraceId.Should().Be("err-456");
        result.Source.Should().Be("Hammer.Auction");
        result.Level.Should().Be("Error");
        result.ExceptionType.Should().Be("System.NullReferenceException");
        result.Message.Should().Be("Object reference not set");
        result.StackTrace.Should().Be("at Hammer.Auction.Controllers.AuctionController.Get()");
        result.RequestPath.Should().Be("/api/auctions/1");
        result.RequestMethod.Should().Be("GET");
        result.Timestamp.Should().Be(DateTimeOffset.Parse("2026-03-22T13:00:00+09:00", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ShouldDeserializeGatewayRequestLogWithNullableFieldsMissing()
    {
        // Arrange — minimal payload (only required fields)
        var json = """
        {
            "traceId": "minimal-1",
            "method": "DELETE",
            "path": "/api/items/99",
            "statusCode": 204,
            "durationMs": 10,
            "timestamp": "2026-03-22T14:00:00Z"
        }
        """;

        // Act
        GatewayRequestLog? result = JsonSerializer.Deserialize<GatewayRequestLog>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.TraceId.Should().Be("minimal-1");
        result.UserId.Should().BeNull();
        result.QueryString.Should().BeNull();
        result.ClientIp.Should().BeNull();
        result.UserAgent.Should().BeNull();
        result.RequestSize.Should().BeNull();
        result.ResponseSize.Should().BeNull();
        result.RouteCluster.Should().BeNull();
    }
}
