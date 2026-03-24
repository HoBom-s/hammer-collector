using System.IO.Compression;
using dotenv.net;
using Hammer.Collector.Api.Middleware;
using Hammer.Collector.Application;
using Hammer.Collector.Infrastructure;
using Hammer.Collector.Infrastructure.Persistence;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "live";
var envFileName = $".env.{env}";
var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

while (dir is not null && !File.Exists(Path.Combine(dir.FullName, envFileName)))
    dir = dir.Parent;

if (dir is not null)
    DotEnv.Load(new DotEnvOptions(envFilePaths: [Path.Combine(dir.FullName, envFileName)]));
else
    await Console.Error.WriteLineAsync($"Warning: {envFileName} not found in any parent directory.");

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(configuration =>
    configuration.ReadFrom.Configuration(builder.Configuration));

var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];

if (!string.IsNullOrEmpty(otlpEndpoint))
{
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("hammer-collector"))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)));
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter(
        "analytics",
        limiter =>
        {
            limiter.PermitLimit = 100;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueLimit = 0;
        });
});

WebApplication app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using IServiceScope scope = app.Services.CreateScope();
    CollectorDbContext db = scope.ServiceProvider.GetRequiredService<CollectorDbContext>();
    await db.Database.MigrateAsync();
}

app.UseResponseCompression();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "0";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Cache-Control"] = "no-store";

    if (!context.Request.Path.StartsWithSegments("/scalar", StringComparison.OrdinalIgnoreCase))
        context.Response.Headers["Content-Security-Policy"] = "default-src 'none'";

    await next();
});

app.UseExceptionHandler();
app.UseRateLimiter();
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

await app.RunAsync();

#pragma warning disable CA1050, S1118 // Required for WebApplicationFactory<Program>
public partial class Program;
#pragma warning restore CA1050, S1118
