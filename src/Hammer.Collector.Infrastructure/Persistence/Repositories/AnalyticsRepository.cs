using System.Diagnostics.CodeAnalysis;
using Hammer.Collector.Domain.Analytics;
using Hammer.Collector.Domain.Entities;
using Hammer.Collector.Domain.Enums;
using Hammer.Collector.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Collector.Infrastructure.Persistence.Repositories;

[ExcludeFromCodeCoverage]
internal sealed class AnalyticsRepository(CollectorDbContext db) : IAnalyticsRepository
{
    public async Task<TrafficTrendResult> GetTrafficTrendsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        TimeBucket bucket,
        CancellationToken ct = default)
    {
        var interval = ToPostgresInterval(bucket);
        var bucketSeconds = ToBucketSeconds(bucket);

        List<TrendRaw> points = await db.Database
            .SqlQuery<TrendRaw>($"""
                SELECT
                    date_trunc({interval}, timestamp) AS "Bucket",
                    COUNT(*) AS "Count"
                FROM gateway_request_logs
                WHERE timestamp >= {from} AND timestamp < {to}
                GROUP BY 1
                ORDER BY 1
                """)
            .ToListAsync(ct);

        var result = points
            .Select(p => new TrafficTrendPoint(p.Bucket, p.Count, Math.Round(p.Count / bucketSeconds, 2)))
            .ToList();

        return new TrafficTrendResult(result);
    }

    public async Task<StatusCodeDistributionResult> GetStatusCodeDistributionAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        TimeBucket bucket,
        CancellationToken ct = default)
    {
        var interval = ToPostgresInterval(bucket);

        IQueryable<GatewayRequestLog> filtered = db.GatewayRequestLogs
            .AsNoTracking()
            .Where(l => l.Timestamp >= from && l.Timestamp < to);

        List<StatusCodeSummaryRaw> summaryData = await filtered
            .GroupBy(l => l.StatusCode / 100)
            .Select(g => new StatusCodeSummaryRaw { StatusCodeClass = g.Key, Count = g.LongCount() })
            .ToListAsync(ct);

        List<StatusCodeTimeSeriesRaw> timeSeriesData = await db.Database
            .SqlQuery<StatusCodeTimeSeriesRaw>($"""
                SELECT
                    date_trunc({interval}, timestamp) AS "Bucket",
                    status_code / 100 AS "StatusCodeClass",
                    COUNT(*) AS "Count"
                FROM gateway_request_logs
                WHERE timestamp >= {from} AND timestamp < {to}
                GROUP BY 1, 2
                ORDER BY 1, 2
                """)
            .ToListAsync(ct);

        var total = summaryData.Sum(s => s.Count);
        var summary = summaryData
            .Select(s => new StatusCodeSummary(
                s.StatusCodeClass,
                s.Count,
                total > 0 ? Math.Round(s.Count * 100.0 / total, 2) : 0))
            .OrderBy(s => s.StatusCodeClass)
            .ToList();

        var timeSeries = timeSeriesData
            .Select(r => new StatusCodeBucket(r.Bucket, r.StatusCodeClass, r.Count))
            .ToList();

        return new StatusCodeDistributionResult(summary, timeSeries);
    }

    public async Task<LatencyAnalysisResult> GetLatencyAnalysisAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct = default)
    {
        LatencyRaw? result = await db.Database
            .SqlQuery<LatencyRaw>($"""
                SELECT
                    COALESCE(AVG(duration_ms), 0) AS "AvgMs",
                    COALESCE(MAX(duration_ms), 0) AS "MaxMs",
                    COALESCE(percentile_cont(0.5) WITHIN GROUP (ORDER BY duration_ms), 0) AS "P50Ms",
                    COALESCE(percentile_cont(0.95) WITHIN GROUP (ORDER BY duration_ms), 0) AS "P95Ms",
                    COALESCE(percentile_cont(0.99) WITHIN GROUP (ORDER BY duration_ms), 0) AS "P99Ms",
                    COUNT(*) AS "TotalRequests"
                FROM gateway_request_logs
                WHERE timestamp >= {from} AND timestamp < {to}
                """)
            .FirstOrDefaultAsync(ct);

        if (result is null)
            return new LatencyAnalysisResult(0, 0, 0, 0, 0, 0);

        return new LatencyAnalysisResult(
            Math.Round(result.AvgMs, 2),
            result.MaxMs,
            Math.Round(result.P50Ms, 2),
            Math.Round(result.P95Ms, 2),
            Math.Round(result.P99Ms, 2),
            result.TotalRequests);
    }

    public async Task<SlowEndpointResult> GetSlowEndpointsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int top,
        CancellationToken ct = default)
    {
        List<SlowEndpointEntry> endpoints = await db.GatewayRequestLogs
            .AsNoTracking()
            .Where(l => l.Timestamp >= from && l.Timestamp < to)
            .GroupBy(l => new { l.Method, l.Path })
            .Select(g => new SlowEndpointEntry(
                g.Key.Method,
                g.Key.Path,
                Math.Round(g.Average(l => (double)l.DurationMs), 2),
                g.Max(l => l.DurationMs),
                g.LongCount()))
            .OrderByDescending(e => e.AvgMs)
            .Take(top)
            .ToListAsync(ct);

        return new SlowEndpointResult(endpoints);
    }

    public async Task<ErrorTrendResult> GetErrorTrendAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        TimeBucket bucket,
        CancellationToken ct = default)
    {
        var interval = ToPostgresInterval(bucket);

        List<TrendRaw> points = await db.Database
            .SqlQuery<TrendRaw>($"""
                SELECT
                    date_trunc({interval}, timestamp) AS "Bucket",
                    COUNT(*) AS "Count"
                FROM service_error_logs
                WHERE timestamp >= {from} AND timestamp < {to}
                GROUP BY 1
                ORDER BY 1
                """)
            .ToListAsync(ct);

        var result = points
            .Select(p => new ErrorTrendPoint(p.Bucket, p.Count))
            .ToList();

        return new ErrorTrendResult(result);
    }

    public async Task<ErrorDistributionResult> GetErrorDistributionAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct = default)
    {
        IQueryable<ServiceErrorLog> filtered = db.ServiceErrorLogs
            .AsNoTracking()
            .Where(l => l.Timestamp >= from && l.Timestamp < to);

        List<ErrorDistributionEntry> byType = await filtered
            .GroupBy(l => l.ExceptionType)
            .Select(g => new ErrorDistributionEntry(g.Key, g.LongCount()))
            .OrderByDescending(e => e.Count)
            .ToListAsync(ct);

        List<ErrorDistributionEntry> bySource = await filtered
            .GroupBy(l => l.Source)
            .Select(g => new ErrorDistributionEntry(g.Key, g.LongCount()))
            .OrderByDescending(e => e.Count)
            .ToListAsync(ct);

        List<ErrorDistributionEntry> byLevel = await filtered
            .GroupBy(l => l.Level)
            .Select(g => new ErrorDistributionEntry(g.Key, g.LongCount()))
            .OrderByDescending(e => e.Count)
            .ToListAsync(ct);

        return new ErrorDistributionResult(byType, bySource, byLevel);
    }

    public async Task<ErrorListResult> GetRecentErrorsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        IQueryable<ServiceErrorLog> filtered = db.ServiceErrorLogs
            .AsNoTracking()
            .Where(l => l.Timestamp >= from && l.Timestamp < to);

        var totalCount = await filtered.LongCountAsync(ct);

        List<ErrorDetail> errors = await filtered
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

    private static string ToPostgresInterval(TimeBucket bucket) => bucket switch
    {
        TimeBucket.Minute => "minute",
        TimeBucket.Hour => "hour",
        TimeBucket.Day => "day",
        _ => "hour",
    };

    private static double ToBucketSeconds(TimeBucket bucket) => bucket switch
    {
        TimeBucket.Minute => 60.0,
        TimeBucket.Hour => 3600.0,
        TimeBucket.Day => 86400.0,
        _ => 3600.0,
    };

    private sealed record TrendRaw(DateTimeOffset Bucket, long Count);

    private sealed record StatusCodeTimeSeriesRaw(DateTimeOffset Bucket, int StatusCodeClass, long Count);

    private sealed class StatusCodeSummaryRaw
    {
        public int StatusCodeClass { get; init; }

        public long Count { get; init; }
    }

    private sealed record LatencyRaw(
        double AvgMs,
        long MaxMs,
        double P50Ms,
        double P95Ms,
        double P99Ms,
        long TotalRequests);
}
