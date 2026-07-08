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

        /// <summary>
        /// Initializes a new instance of the ProcessOrderToSql class.
        /// </summary>
        /// <param name="dbContextFactory">The DbContext factory for creating OrderIntegrationContext instances.</param>
        public ProcessOrderToSql(IDbContextFactory<OrderIntegrationContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        /// <summary>
        /// Processes a queue message containing order data and inserts it into the SQL database using EF Core.
        /// </summary>
        /// <param name="message">The queue message containing serialized OrderRequest JSON.</param>
        /// <param name="context">The function context providing access to the logger.</param>
        [Function("ProcessOrderToSql")]
        public async Task Run(
            [QueueTrigger("orders-queue", Connection = "AzureWebJobsStorage")] string message,
            FunctionContext context)
        {
            var logger = context.GetLogger("ProcessOrderToSql");

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

                // Insert order into database using EF Core
                await InsertOrderIntoDatabaseAsync(orderRequest, logger);

                logger.LogInformation("Order {OrderId} successfully persisted to SQL database", orderRequest.OrderId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing queue message: {Message}", message);
                throw;
            }
        }

        /// <summary>
        /// Inserts an order into the SQL database using Entity Framework Core.
        /// </summary>
        /// <param name="orderRequest">The order request to insert.</param>
        /// <param name="logger">The logger instance.</param>
        private async Task InsertOrderIntoDatabaseAsync(OrderRequest orderRequest, ILogger logger)
        {
            try
            {
                // Create a new context instance for this operation
                using var context = _dbContextFactory.CreateDbContext();

                // Create a new Order entity from the request
                var order = new Order
                {
                    OrderId = orderRequest.OrderId ?? string.Empty,
                    CustomerId = orderRequest.CustomerId ?? string.Empty,
                    Total = orderRequest.Total,
                    Description = orderRequest.Description,
                    OrderDate = orderRequest.OrderDate ?? DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // Add the order to the DbSet
                context.Orders.Add(order);

                // Save changes to the database asynchronously
                int rowsAffected = await context.SaveChangesAsync();

                if (rowsAffected == 0)
                {
                    logger.LogWarning("No rows were affected during insert for order {OrderId}", orderRequest.OrderId);
                }
                else
                {
                    logger.LogInformation("Successfully inserted {RowsAffected} row(s) for order {OrderId}", 
                        rowsAffected, orderRequest.OrderId);
                }
            }
            catch (DbUpdateException dbEx)
            {
                // DbUpdateConcurrencyException is derived from DbUpdateException,
                // so we check for it first (though it's caught as DbUpdateException)
                if (dbEx is DbUpdateConcurrencyException)
                {
                    logger.LogError(dbEx, "Concurrency error occurred while inserting order {OrderId}", 
                        orderRequest.OrderId);
                }
                else
                {
                    logger.LogError(dbEx, "Database error occurred while inserting order {OrderId}: {DbError}", 
                        orderRequest.OrderId, dbEx.InnerException?.Message);
                }
                throw new InvalidOperationException($"Failed to insert order into database: {dbEx.InnerException?.Message}", dbEx);
            }
            catch (OperationCanceledException cancelEx)
            {
                logger.LogError(cancelEx, "Operation was cancelled while inserting order {OrderId}", 
                    orderRequest.OrderId);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while inserting order {OrderId}", orderRequest.OrderId);
                throw;
            }
        }
    }
}

