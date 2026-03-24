using Hammer.Collector.Domain.Enums;

namespace Hammer.Collector.Application.UseCases.Analytics.GetErrorTrend;

public sealed record GetErrorTrendRequest(
    DateTimeOffset From,
    DateTimeOffset To,
    TimeBucket Bucket = TimeBucket.Hour);
