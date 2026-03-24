namespace Hammer.Collector.Application.UseCases.Search.SearchRequestLogs;

public sealed record SearchRequestLogsRequest(
    DateTimeOffset From,
    DateTimeOffset To,
    string? TraceId = null,
    int? StatusCode = null,
    string? Method = null,
    string? Path = null,
    int Page = 1,
    int PageSize = 20);
