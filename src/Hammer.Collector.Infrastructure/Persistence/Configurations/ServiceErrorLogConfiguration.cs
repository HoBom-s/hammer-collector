using System.Diagnostics.CodeAnalysis;
using Hammer.Collector.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Collector.Infrastructure.Persistence.Configurations;

[ExcludeFromCodeCoverage]
public sealed class ServiceErrorLogConfiguration : IEntityTypeConfiguration<ServiceErrorLog>
{
    public void Configure(EntityTypeBuilder<ServiceErrorLog> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TraceId).IsRequired().HasMaxLength(64);
        builder.Property(e => e.Source).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Level).IsRequired().HasMaxLength(16);
        builder.Property(e => e.ExceptionType).IsRequired().HasMaxLength(512);
        builder.Property(e => e.Message).IsRequired();
        builder.Property(e => e.RequestPath).IsRequired().HasMaxLength(2048);
        builder.Property(e => e.RequestMethod).IsRequired().HasMaxLength(10);

        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.TraceId);
        builder.HasIndex(e => new { e.Timestamp, e.ExceptionType });
        builder.HasIndex(e => new { e.Timestamp, e.Source });
    }
}
