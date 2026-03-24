namespace Hammer.Collector.Application.UseCases.Analytics.GetErrorDistribution;

public sealed record GetErrorDistributionRequest(
    DateTimeOffset From,
    DateTimeOffset To);
