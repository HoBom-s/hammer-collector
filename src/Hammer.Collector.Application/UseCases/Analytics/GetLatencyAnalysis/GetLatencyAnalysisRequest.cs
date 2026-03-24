namespace Hammer.Collector.Application.UseCases.Analytics.GetLatencyAnalysis;

public sealed record GetLatencyAnalysisRequest(
    DateTimeOffset From,
    DateTimeOffset To);
