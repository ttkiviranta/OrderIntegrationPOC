using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OrderFunctionApp.Data
{
    /// <summary>
    /// DesignTimeDbContextFactory provides the DbContext instance at design time for EF Core tools.
    /// This is required for running migrations from the dotnet-ef CLI tool.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrderIntegrationContext>
    {
        /// <summary>
        /// Creates a new DbContext instance for design-time operations (migrations).
        /// </summary>
        /// <param name="args">Command-line arguments (not used).</param>
        /// <returns>A configured OrderIntegrationContext instance.</returns>
        public OrderIntegrationContext CreateDbContext(string[] args)
        {
            // Load configuration from local.settings.json for migrations
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Create connection string for local SQL Express database
            // Ensure no encryption is used for local connections
            var connectionString = "Server=TimoK\\SQLEXPRESS;Database=OrderIntegrationPOC_DB;Trusted_Connection=True;MultipleActiveResultSets=True;Encrypt=False;";

            var optionsBuilder = new DbContextOptionsBuilder<OrderIntegrationContext>();
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
            });

            return new OrderIntegrationContext(optionsBuilder.Options);
        }
    }
}
