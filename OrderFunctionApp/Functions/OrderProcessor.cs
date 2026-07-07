using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using OrderFunctionApp.Models;

namespace OrderFunctionApp.Functions
{
    /// <summary>
    /// OrderProcessor handles the processing and queueing of order requests.
    /// It receives OrderRequest objects, serializes them to JSON, and sends them to Azure Storage Queue.
    /// </summary>
    public class OrderProcessor
    {
        private readonly ILogger<OrderProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of the OrderProcessor class.
        /// </summary>
        /// <param name="logger">The logger instance for this processor.</param>
        public OrderProcessor(ILogger<OrderProcessor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Processes an order request by sending it to the Azure Storage Queue.
        /// </summary>
        /// <param name="orderRequest">The order request to process.</param>
        /// <param name="connectionString">The connection string for Azure Storage Queue (supports both real storage and Azure Storage Emulator).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when orderRequest or connectionString is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when queue operations fail.</exception>
        public async Task ProcessAsync(OrderRequest orderRequest, string connectionString)
        {
            if (orderRequest == null)
            {
                _logger.LogError("OrderRequest is null");
                throw new ArgumentNullException(nameof(orderRequest), "Order request cannot be null");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogError("Connection string is null or empty");
                throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty");
            }

            try
            {
                _logger.LogInformation("Processing order request with OrderId: {OrderId}", orderRequest.OrderId);

                // Create Queue client using connection string (works with both Azure Storage and Storage Emulator)
                var queueClient = new QueueClient(connectionString, "orders-queue");

                // Ensure queue exists
                await queueClient.CreateIfNotExistsAsync();

                // Serialize the order request to JSON
                var options = new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };
                var messageContent = JsonSerializer.Serialize(orderRequest, options);

                // Send message to queue
                await queueClient.SendMessageAsync(messageContent);

                _logger.LogInformation("Order processed successfully. OrderId: {OrderId}, QueuedTime: {Now}", 
                    orderRequest.OrderId, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order request with OrderId: {OrderId}", orderRequest?.OrderId);
                throw new InvalidOperationException("Failed to process order request", ex);
            }
        }

    }
}


