using System.Diagnostics.CodeAnalysis;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Entities;
using Hammer.Collector.Domain.Ports;
using Hammer.Collector.Domain.Search;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Collector.Infrastructure.Persistence.Repositories;

[ExcludeFromCodeCoverage]
internal sealed class SearchRepository(CollectorDbContext db) : ISearchRepository
{
    public async Task<RequestLogSearchResult> SearchRequestLogsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        string? traceId,
        int? statusCode,
        string? method,
        string? path,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        IQueryable<GatewayRequestLog> query = db.GatewayRequestLogs
            .AsNoTracking()
            .Where(l => l.Timestamp >= from && l.Timestamp < to);

        if (!string.IsNullOrEmpty(traceId))
            query = query.Where(l => l.TraceId == traceId);

        if (statusCode.HasValue)
            query = query.Where(l => l.StatusCode == statusCode.Value);

        if (!string.IsNullOrEmpty(method))
            query = query.Where(l => l.Method == method);

        if (!string.IsNullOrEmpty(path))
            query = query.Where(l => l.Path.Contains(path));

        var totalCount = await query.LongCountAsync(ct);

        List<RequestLogDetail> logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new RequestLogDetail(
                l.Id,
                l.TraceId,
                l.UserId,
                l.Method,
                l.Path,
                l.QueryString,
                l.StatusCode,
                l.DurationMs,
                l.ClientIp,
                l.UserAgent,
                l.RouteCluster,
                l.Timestamp))
            .ToListAsync(ct);

        return new RequestLogSearchResult(logs, totalCount, page, pageSize);
    }

    public async Task<ErrorListResult> SearchErrorLogsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        string? traceId,
        string? exceptionType,
        string? source,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        IQueryable<ServiceErrorLog> query = db.ServiceErrorLogs
            .AsNoTracking()
            .Where(l => l.Timestamp >= from && l.Timestamp < to);

        if (!string.IsNullOrEmpty(traceId))
            query = query.Where(l => l.TraceId == traceId);

        if (!string.IsNullOrEmpty(exceptionType))
            query = query.Where(l => l.ExceptionType == exceptionType);

        if (!string.IsNullOrEmpty(source))
            query = query.Where(l => l.Source == source);

        var totalCount = await query.LongCountAsync(ct);

        List<ErrorDetail> errors = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new ErrorDetail(
                l.Id,
                l.TraceId,
                l.Source,
                l.Level,
                l.ExceptionType,
                l.Message,
                l.StackTrace,
                l.RequestPath,
                l.RequestMethod,
                l.Timestamp))
            .ToListAsync(ct);

        return new ErrorListResult(errors, totalCount, page, pageSize);
    }

    public async Task<TraceSearchResult> SearchByTraceIdAsync(
        string traceId,
        CancellationToken ct = default)
    {
        List<RequestLogDetail> requestLogs = await db.GatewayRequestLogs
            .AsNoTracking()
            .Where(l => l.TraceId == traceId)
            .OrderByDescending(l => l.Timestamp)
            .Select(l => new RequestLogDetail(
                l.Id,
                l.TraceId,
                l.UserId,
                l.Method,
                l.Path,
                l.QueryString,
                l.StatusCode,
                l.DurationMs,
                l.ClientIp,
                l.UserAgent,
                l.RouteCluster,
                l.Timestamp))
            .ToListAsync(ct);

        List<ErrorDetail> errorLogs = await db.ServiceErrorLogs
            .AsNoTracking()
            .Where(l => l.TraceId == traceId)
            .OrderByDescending(l => l.Timestamp)
            .Select(l => new ErrorDetail(
                l.Id,
                l.TraceId,
                l.Source,
                l.Level,
                l.ExceptionType,
                l.Message,
                l.StackTrace,
                l.RequestPath,
                l.RequestMethod,
                l.Timestamp))
            .ToListAsync(ct);

        return new TraceSearchResult(requestLogs, errorLogs);
    }
}
