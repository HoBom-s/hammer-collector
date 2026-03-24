using Hammer.Collector.Domain.Ports;
using Hammer.Collector.Infrastructure.Kafka;
using Hammer.Collector.Infrastructure.Persistence;
using Hammer.Collector.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hammer.Collector.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CollectorDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

        services.AddSingleton<IKafkaMessageHandler, GatewayRequestLogHandler>();
        services.AddSingleton<IKafkaMessageHandler, ServiceErrorLogHandler>();
        services.AddHostedService<KafkaConsumerWorker>();

        services
            .AddHealthChecks()
            .AddDbContextCheck<CollectorDbContext>();

        return services;
    }
}
