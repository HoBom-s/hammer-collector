using Hammer.Collector.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Collector.Infrastructure.Persistence;

public sealed class CollectorDbContext(DbContextOptions<CollectorDbContext> options)
    : DbContext(options)
{
    public DbSet<GatewayRequestLog> GatewayRequestLogs => Set<GatewayRequestLog>();

    public DbSet<ServiceErrorLog> ServiceErrorLogs => Set<ServiceErrorLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CollectorDbContext).Assembly);
    }
}
