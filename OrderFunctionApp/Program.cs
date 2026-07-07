using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderFunctionApp.Functions;

var builder = FunctionsApplication.CreateBuilder(args);

// Load configuration from local.settings.json and environment variables
builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();

// Add Functions Worker defaults (required for .NET 8 isolated model)
builder.Services.AddFunctionsWorkerDefaults();

// Add Application Insights for telemetry and monitoring
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Register OrderProcessor for dependency injection
builder.Services.AddScoped<OrderProcessor>();

// Configure logging
builder.Services.AddLogging(configure =>
{
    configure.AddApplicationInsights();
    configure.SetMinimumLevel(LogLevel.Information);
});

builder.Build().Run();



