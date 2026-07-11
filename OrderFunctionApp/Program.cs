 using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderFunctionApp.Data;
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

// Register DbContextFactory for Entity Framework Core (Azure Functions pattern)
// DbContextFactory is used instead of AddDbContext because Azure Functions
// requires creating new context instances per invocation
builder.Services.AddDbContextFactory<OrderIntegrationContext>(options =>
{
    var connectionString = builder.Configuration["SqlConnectionString"];

    // Use provided connection string or fallback to local SQL Express
    if (string.IsNullOrEmpty(connectionString))
    {
        connectionString = "Server=TimoK\\SQLEXPRESS;Database=OrderIntegrationPOC_DB;Trusted_Connection=True;MultipleActiveResultSets=True;Encrypt=False;";
    }

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(30);
    });
});


// Register OrderProcessor for dependency injection
builder.Services.AddScoped<OrderProcessor>();

// Register ProcessOrderToSql for dependency injection
builder.Services.AddScoped<ProcessOrderToSql>();

// Configure logging
builder.Services.AddLogging(configure =>
{
    configure.AddApplicationInsights();
    configure.SetMinimumLevel(LogLevel.Information);
});

builder.Build().Run();



