using Microsoft.Extensions.DependencyInjection;

namespace Hammer.Collector.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
