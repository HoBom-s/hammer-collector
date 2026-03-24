namespace Hammer.Collector.Application.UseCases.Analytics.GetRecentErrors;

public sealed record GetRecentErrorsRequest(
    DateTimeOffset From,
    DateTimeOffset To,
    int Page = 1,
    int PageSize = 20);
