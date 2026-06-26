using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

// Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();

// Add Application Insights for telemetry and monitoring
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configure logging
builder.Services.AddLogging(configure =>
{
    configure.AddApplicationInsights();
    configure.SetMinimumLevel(LogLevel.Information);
});

builder.Build().Run();
