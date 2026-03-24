using Hammer.Collector.Domain.Enums;

namespace Hammer.Collector.Application.UseCases.Analytics.GetStatusCodeDistribution;

public sealed record GetStatusCodeDistributionRequest(
    DateTimeOffset From,
    DateTimeOffset To,
    TimeBucket Bucket = TimeBucket.Hour);
