namespace Hammer.Collector.Application.UseCases.Search.SearchErrorLogs;

public sealed record SearchErrorLogsRequest(
    DateTimeOffset From,
    DateTimeOffset To,
    string? TraceId = null,
    string? ExceptionType = null,
    string? Source = null,
    int Page = 1,
    int PageSize = 20);
