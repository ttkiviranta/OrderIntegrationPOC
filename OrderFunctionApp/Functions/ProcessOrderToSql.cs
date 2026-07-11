using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFunctionApp.Data;
using OrderFunctionApp.Models;

namespace OrderFunctionApp.Functions
{
    /// <summary>
    /// ProcessOrderToSql is a Queue-triggered Azure Function that processes orders from the queue
    /// and persists them to an Azure SQL Database using Entity Framework Core.
    /// </summary>
    public class ProcessOrderToSql
    {
        private readonly IDbContextFactory<OrderIntegrationContext> _dbContextFactory;

        public ProcessOrderToSql(IDbContextFactory<OrderIntegrationContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        [Function("ProcessOrderToSql")]
        public async Task Run(
            [QueueTrigger("orders-queue", Connection = "AzureWebJobsStorage")] string message,
            FunctionContext context)
        {
            var logger = context.GetLogger("ProcessOrderToSql");

            logger.LogInformation("=== ProcessOrderToSql TRIGGERED ===");
            logger.LogInformation("Queue message received: {Message}", message);

            try
            {
                logger.LogInformation("Processing queue message at {Timestamp}", DateTime.UtcNow);

                // Deserialize the queue message into OrderRequest
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var orderRequest = JsonSerializer.Deserialize<OrderRequest>(message, options);

                if (orderRequest == null)
                {
                    logger.LogError("Failed to deserialize queue message: {Message}", message);
                    throw new InvalidOperationException("Queue message deserialization failed");
                }

                logger.LogInformation("Inserting order {OrderId} into database using Entity Framework Core", orderRequest.OrderId);

                // Insert order into database
                using var dbContext = _dbContextFactory.CreateDbContext();

                var order = new Order
                {
                    OrderId = orderRequest.OrderId ?? string.Empty,
                    CustomerId = orderRequest.CustomerId ?? string.Empty,
                    Total = orderRequest.Total,
                    Description = orderRequest.Description ?? string.Empty,
                    OrderDate = orderRequest.OrderDate ?? DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.Orders.Add(order);
                int rowsAffected = await dbContext.SaveChangesAsync();

                logger.LogInformation("Successfully inserted {RowsAffected} row(s) for order {OrderId}", rowsAffected, orderRequest.OrderId);
                logger.LogInformation("Order {OrderId} successfully persisted to SQL database", orderRequest.OrderId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing queue message: {Message}", message);
                throw;
            }
        }
    }
}
