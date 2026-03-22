using dotenv.net;
using Hammer.Collector.Application;
using Hammer.Collector.Infrastructure;
using Hammer.Collector.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

#pragma warning disable CA1308 // Normalize strings to uppercase — lowercase needed for .env file naming
DotEnv.Load(options: new DotEnvOptions(envFilePaths: [$".env.{environment.ToLowerInvariant()}"]));
#pragma warning restore CA1308

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection")!);

builder.Services
    .AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    CollectorDbContext db = scope.ServiceProvider.GetRequiredService<CollectorDbContext>();
    await db.Database.MigrateAsync();
}

app.MapHealthChecks("/health");

await app.RunAsync();
