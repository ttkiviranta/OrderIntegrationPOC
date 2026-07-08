using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderFunctionApp.Models;

namespace OrderFunctionApp.Functions
{
    /// <summary>
    /// ProcessOrderToSql is a Queue-triggered Azure Function that processes orders from the queue
    /// and persists them to an Azure SQL Database.
    /// </summary>
    public class ProcessOrderToSql
    {
        private readonly string _sqlConnectionString;

        /// <summary>
        /// Initializes a new instance of the ProcessOrderToSql class.
        /// </summary>
        /// <param name="configuration">The configuration containing the SQL connection string.</param>
        public ProcessOrderToSql(IConfiguration configuration)
        {
            _sqlConnectionString = configuration["SqlConnectionString"] 
                ?? throw new InvalidOperationException("SqlConnectionString is not configured");
        }

        /// <summary>
        /// Processes a queue message containing order data and inserts it into the SQL database.
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
                var order = JsonSerializer.Deserialize<OrderRequest>(message, options);

                if (order == null)
                {
                    logger.LogError("Failed to deserialize queue message: {Message}", message);
                    throw new InvalidOperationException("Queue message deserialization failed");
                }

                logger.LogInformation("Inserting order {OrderId} into SQL database", order.OrderId);

                // Insert order into SQL database
                await InsertOrderIntoDatabase(order, logger);

                logger.LogInformation("Order {OrderId} successfully persisted to SQL database", order.OrderId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing queue message: {Message}", message);
                throw;
            }
        }

        /// <summary>
        /// Inserts an order into the SQL database.
        /// </summary>
        /// <param name="order">The order to insert.</param>
        /// <param name="logger">The logger instance.</param>
        private async Task InsertOrderIntoDatabase(OrderRequest order, ILogger logger)
        {
            try
            {
                using var connection = new SqlConnection(_sqlConnectionString);
                await connection.OpenAsync();

                const string insertQuery = @"
                    INSERT INTO Orders (OrderId, CustomerId, Total, Description, OrderDate)
                    VALUES (@OrderId, @CustomerId, @Total, @Description, @OrderDate)";

                using var command = new SqlCommand(insertQuery, connection);

                // Add parameters to prevent SQL injection
                command.Parameters.AddWithValue("@OrderId", order.OrderId ?? "");
                command.Parameters.AddWithValue("@CustomerId", order.CustomerId ?? "");
                command.Parameters.AddWithValue("@Total", order.Total);
                command.Parameters.AddWithValue("@Description", order.Description ?? "");
                command.Parameters.AddWithValue("@OrderDate", order.OrderDate ?? DateTime.UtcNow);

                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    logger.LogWarning("No rows were affected during insert for order {OrderId}", order.OrderId);
                }
                else
                {
                    logger.LogInformation("Successfully inserted 1 row for order {OrderId}", order.OrderId);
                }
            }
            catch (SqlException sqlEx)
            {
                logger.LogError(sqlEx, "SQL error occurred while inserting order {OrderId}: {SqlError}", 
                    order.OrderId, sqlEx.Message);
                throw new InvalidOperationException($"Failed to insert order into database: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while inserting order {OrderId}", order.OrderId);
                throw;
            }
        }
    }
}
