using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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
            var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")
                ?? "Server=tcp:orderintegrationpoc-sqlserver.database.windows.net,1433;Initial Catalog=OrderIntegrationPOC_DB;Persist Security Info=False;User ID=orderadmin;Password=PocDemo!2024;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

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
