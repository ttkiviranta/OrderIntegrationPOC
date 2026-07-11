using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFunctionApp.Data;
using OrderFunctionApp.Models;

namespace OrderFunctionApp.Functions
{
    /// <summary>
    /// InsertOrderDirectly is an HTTP-triggered Azure Function for testing database writes directly.
    /// This function bypasses the queue and writes directly to the SQL database.
    /// </summary>
    public class InsertOrderDirectly
    {
        private readonly IDbContextFactory<OrderIntegrationContext> _dbContextFactory;

        public InsertOrderDirectly(IDbContextFactory<OrderIntegrationContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        [Function("InsertOrderDirectly")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/direct")] HttpRequestData req,
            FunctionContext context)
        {
            var logger = context.GetLogger("InsertOrderDirectly");
            logger.LogInformation("=== InsertOrderDirectly function TRIGGERED ===");

            try
            {
                // Read request body
                var requestBody = await req.ReadAsStringAsync();
                logger.LogInformation("Request body: {RequestBody}", requestBody);

                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    logger.LogError("Request body is empty");
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteAsJsonAsync(new { error = "Request body cannot be empty" });
                    return errorResponse;
                }

                // Deserialize order
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };
                var orderRequest = System.Text.Json.JsonSerializer.Deserialize<OrderRequest>(requestBody, options);
                logger.LogInformation("Deserialized order: OrderId={OrderId}, CustomerId={CustomerId}", 
                    orderRequest?.OrderId, orderRequest?.CustomerId);

                if (orderRequest == null)
                {
                    logger.LogError("Failed to deserialize request");
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteAsJsonAsync(new { error = "Invalid order request" });
                    return errorResponse;
                }

                logger.LogInformation("Creating DbContext...");
                using var dbContext = _dbContextFactory.CreateDbContext();
                logger.LogInformation("DbContext created successfully");

                // Create order
                var order = new Order
                {
                    OrderId = orderRequest.OrderId ?? string.Empty,
                    CustomerId = orderRequest.CustomerId ?? string.Empty,
                    Total = orderRequest.Total,
                    Description = orderRequest.Description ?? string.Empty,
                    OrderDate = orderRequest.OrderDate ?? DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                logger.LogInformation("Order object created: {OrderId}={OrderId}, {CustomerId}={CustomerId}, {Total}={Total}",
                    "OrderId", order.OrderId, "CustomerId", order.CustomerId, "Total", order.Total);

                logger.LogInformation("Adding order to DbSet...");
                dbContext.Orders.Add(order);
                logger.LogInformation("Order added to DbSet");

                logger.LogInformation("Calling SaveChangesAsync()...");
                int rowsAffected = await dbContext.SaveChangesAsync();
                logger.LogInformation("SaveChangesAsync completed. Rows affected: {RowsAffected}", rowsAffected);

                if (rowsAffected == 0)
                {
                    logger.LogWarning("WARNING: SaveChangesAsync returned 0 rows affected!");
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new
                {
                    success = true,
                    message = $"Order inserted directly to database (rows affected: {rowsAffected})",
                    orderId = orderRequest.OrderId,
                    rowsAffected = rowsAffected,
                    timestamp = DateTime.UtcNow.ToString("O")
                });

                logger.LogInformation("=== InsertOrderDirectly COMPLETED SUCCESSFULLY ===");
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ERROR in InsertOrderDirectly: {ExceptionMessage}", ex.Message);
                logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);

                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new
                {
                    error = "Failed to insert order",
                    details = ex.Message,
                    exceptionType = ex.GetType().Name,
                    timestamp = DateTime.UtcNow.ToString("O")
                });
                return errorResponse;
            }
        }
    }
}
