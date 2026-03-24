using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hammer.Collector.Infrastructure.Persistence;

public sealed class CollectorDbContextFactory : IDesignTimeDbContextFactory<CollectorDbContext>
{
    public CollectorDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<CollectorDbContext> optionsBuilder = new();

#pragma warning disable S2068 // Hard-coded credential for design-time migration only
        optionsBuilder
            .UseNpgsql("Host=localhost;Port=5432;Database=hammer;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention();
#pragma warning restore S2068

        return new CollectorDbContext(optionsBuilder.Options);
    }
}
