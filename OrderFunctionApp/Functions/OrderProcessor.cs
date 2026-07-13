using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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
        /// <param name="configuration">The configuration instance for reading Azure Storage Credentials.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when orderRequest or configuration is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when queue operations fail.</exception>
        public async Task ProcessAsync(OrderRequest orderRequest, IConfiguration configuration)
        {
            if (orderRequest == null)
            {
                _logger.LogError("OrderRequest is null");
                throw new ArgumentNullException(nameof(orderRequest), "Order request cannot be null");
            }

            if (configuration == null)
            {
                _logger.LogError("Configuration is null");
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            }

            try
            {
                _logger.LogInformation("Processing order request with OrderId: {OrderId}", orderRequest.OrderId);

                var storageConnectionString = configuration["AzureWebJobsStorage"];
                if (string.IsNullOrWhiteSpace(storageConnectionString))
                {
                    throw new InvalidOperationException("AzureWebJobsStorage configuration is missing.");
                }

                _logger.LogInformation("Creating queue client using AzureWebJobsStorage connection string");
                var queueClient = new QueueClient(
                    storageConnectionString,
                    "orders-queue",
                    new QueueClientOptions
                    {
                        MessageEncoding = QueueMessageEncoding.Base64
                    });

                _logger.LogInformation("Ensuring queue exists...");
                await queueClient.CreateIfNotExistsAsync();
                _logger.LogInformation("Queue ready");

                // Serialize the order request to JSON
                var options = new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };
                var messageContent = JsonSerializer.Serialize(orderRequest, options);

                _logger.LogInformation("Sending message to queue - OrderId: {OrderId}, Size: {Size} bytes", 
                    orderRequest.OrderId, messageContent.Length);

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


