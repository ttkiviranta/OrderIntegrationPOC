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
using OrderFunctionApp.Data.Repositories;
using OrderFunctionApp.Functions;
using OrderFunctionApp.Models.Mappings;
using OrderFunctionApp.Services;
using OrderFunctionApp.Validation;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

var builder = FunctionsApplication.CreateBuilder(args);

// Load configuration from local.settings.json and environment variables
builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Configure Key Vault for production environments
// This loads additional configuration from Azure Key Vault when deployed
var keyVaultUrl = builder.Configuration["KeyVaultUrl"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    try
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
    }
    catch (Exception ex)
    {
        var logger = LoggerFactory.Create(c => c.AddConsole()).CreateLogger("Program");
        logger.LogWarning(ex, "Failed to load configuration from Key Vault: {Message}. Continuing with local configuration.", ex.Message);
    }
}

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

// Register existing Function services
builder.Services.AddScoped<OrderProcessor>();
builder.Services.AddScoped<ProcessOrderToSql>();

// Register new Function services
builder.Services.AddScoped<ServiceBusOrderIngestion>();

// Register Repository pattern implementations
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderLineRepository, OrderLineRepository>();

// Register AutoMapper for DTO mapping
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Register service utilities
builder.Services.AddScoped<RetryPolicy>();
builder.Services.AddScoped<PoisonMessageHandler>();

// Configure logging
builder.Services.AddLogging(configure =>
{
    configure.AddApplicationInsights();
    configure.SetMinimumLevel(LogLevel.Information);
});

builder.Build().Run();



