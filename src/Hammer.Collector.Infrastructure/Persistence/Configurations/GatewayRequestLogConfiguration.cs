using Hammer.Collector.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Collector.Infrastructure.Persistence.Configurations;

public sealed class GatewayRequestLogConfiguration : IEntityTypeConfiguration<GatewayRequestLog>
{
    public void Configure(EntityTypeBuilder<GatewayRequestLog> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TraceId).IsRequired().HasMaxLength(64);
        builder.Property(e => e.UserId).HasMaxLength(64);
        builder.Property(e => e.Method).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Path).IsRequired().HasMaxLength(2048);
        builder.Property(e => e.QueryString).HasMaxLength(4096);
        builder.Property(e => e.ClientIp).HasMaxLength(45);
        builder.Property(e => e.UserAgent).HasMaxLength(512);
        builder.Property(e => e.RouteCluster).HasMaxLength(128);

        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.TraceId);
        builder.HasIndex(e => e.RouteCluster);
        builder.HasIndex(e => e.ClientIp);
        builder.HasIndex(e => new { e.Timestamp, e.StatusCode });
        builder.HasIndex(e => new { e.Timestamp, e.Method, e.Path });
    }
}
