using Hammer.Collector.Domain.Enums;

namespace Hammer.Collector.Application.UseCases.Analytics.GetTrafficTrends;

public sealed record GetTrafficTrendsRequest(
    DateTimeOffset From,
    DateTimeOffset To,
    TimeBucket Bucket = TimeBucket.Hour);
