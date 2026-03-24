using System.Diagnostics.CodeAnalysis;
using Hammer.Collector.Application.UseCases.Analytics.GetErrorDistribution;
using Hammer.Collector.Application.UseCases.Analytics.GetErrorTrend;
using Hammer.Collector.Application.UseCases.Analytics.GetLatencyAnalysis;
using Hammer.Collector.Application.UseCases.Analytics.GetRecentErrors;
using Hammer.Collector.Application.UseCases.Analytics.GetSlowEndpoints;
using Hammer.Collector.Application.UseCases.Analytics.GetStatusCodeDistribution;
using Hammer.Collector.Application.UseCases.Analytics.GetTrafficTrends;
using Hammer.Collector.Application.UseCases.Search.SearchByTraceId;
using Hammer.Collector.Application.UseCases.Search.SearchErrorLogs;
using Hammer.Collector.Application.UseCases.Search.SearchRequestLogs;
using Microsoft.Extensions.DependencyInjection;

namespace Hammer.Collector.Application;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGetTrafficTrendsUseCase, GetTrafficTrendsUseCase>();
        services.AddScoped<IGetStatusCodeDistributionUseCase, GetStatusCodeDistributionUseCase>();
        services.AddScoped<IGetLatencyAnalysisUseCase, GetLatencyAnalysisUseCase>();
        services.AddScoped<IGetSlowEndpointsUseCase, GetSlowEndpointsUseCase>();
        services.AddScoped<IGetErrorTrendUseCase, GetErrorTrendUseCase>();
        services.AddScoped<IGetErrorDistributionUseCase, GetErrorDistributionUseCase>();
        services.AddScoped<IGetRecentErrorsUseCase, GetRecentErrorsUseCase>();

        services.AddScoped<ISearchRequestLogsUseCase, SearchRequestLogsUseCase>();
        services.AddScoped<ISearchErrorLogsUseCase, SearchErrorLogsUseCase>();
        services.AddScoped<ISearchByTraceIdUseCase, SearchByTraceIdUseCase>();

        return services;
    }
}
