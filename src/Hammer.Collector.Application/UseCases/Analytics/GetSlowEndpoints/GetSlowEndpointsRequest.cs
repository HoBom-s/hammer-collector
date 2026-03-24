namespace Hammer.Collector.Application.UseCases.Analytics.GetSlowEndpoints;

public sealed record GetSlowEndpointsRequest(
    DateTimeOffset From,
    DateTimeOffset To,
    int Top = 10);
