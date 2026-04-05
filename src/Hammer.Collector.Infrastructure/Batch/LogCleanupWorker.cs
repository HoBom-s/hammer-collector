using System.Diagnostics.CodeAnalysis;
using Hammer.Collector.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hammer.Collector.Infrastructure.Batch;

[ExcludeFromCodeCoverage]
internal sealed partial class LogCleanupWorker : BackgroundService
{
    private const int BatchSize = 1_000;

    private readonly ILogger<LogCleanupWorker> _logger;

    private readonly IServiceScopeFactory _scopeFactory;

    public LogCleanupWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<LogCleanupWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay = GetDelayUntilMidnightUtc();
            await Task.Delay(delay, stoppingToken);

            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                LogCleanupError(_logger, ex);
            }
        }
    }

    private static TimeSpan GetDelayUntilMidnightUtc()
    {
        DateTime now = DateTime.UtcNow;
        DateTime nextMidnight = now.Date.AddDays(1);
        return nextMidnight - now;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Log cleanup completed: deleted {GatewayCount} gateway request logs and {ErrorCount} service error logs")]
    private static partial void LogCleanupCompleted(ILogger logger, int gatewayCount, int errorCount);

    [LoggerMessage(Level = LogLevel.Error, Message = "Log cleanup failed")]
    private static partial void LogCleanupError(ILogger logger, Exception exception);

    private async Task CleanupAsync(CancellationToken ct)
    {
        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddMonths(-3);

        using IServiceScope scope = _scopeFactory.CreateScope();
        CollectorDbContext dbContext = scope.ServiceProvider.GetRequiredService<CollectorDbContext>();

        var gatewayTotal = 0;
        int deleted;

        while (true)
        {
            deleted = await dbContext.GatewayRequestLogs
                .Where(x => x.Timestamp < cutoff)
                .Take(BatchSize)
                .ExecuteDeleteAsync(ct);

            if (deleted == 0)
                break;

            gatewayTotal += deleted;
        }

        var errorTotal = 0;

        while (true)
        {
            deleted = await dbContext.ServiceErrorLogs
                .Where(x => x.Timestamp < cutoff)
                .Take(BatchSize)
                .ExecuteDeleteAsync(ct);

            if (deleted == 0)
                break;

            errorTotal += deleted;
        }

        LogCleanupCompleted(_logger, gatewayTotal, errorTotal);
    }
}
