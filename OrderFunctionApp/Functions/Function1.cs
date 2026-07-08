using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderFunctionApp.Models;
using System.Text;

namespace OrderFunctionApp.Functions
{
    /// <summary>
    /// ReceiveOrder is an HTTP-triggered Azure Function that handles incoming order requests.
    /// It validates the OrderRequest payload and queues it for processing via OrderProcessor.
    /// </summary>
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly OrderProcessor _orderProcessor;
        private readonly string _storageConnectionString;

        /// <summary>
        /// Initializes a new instance of the Function1 class.
        /// </summary>
        /// <param name="logger">The logger instance for this function.</param>
        /// <param name="orderProcessor">The order processor for handling order requests.</param>
        public Function1(ILogger<Function1> logger, OrderProcessor orderProcessor)
        {
            _logger = logger;
            _orderProcessor = orderProcessor;
            // Read connection string from environment
            _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") 
                ?? throw new InvalidOperationException("AzureWebJobsStorage connection string not found in configuration");
        }

        /// <summary>
        /// Handles HTTP POST requests to receive and process orders.
        /// Expects a JSON payload conforming to the OrderRequest model.
        /// </summary>
        /// <param name="req">The HTTP request containing the order data.</param>
        /// <returns>HTTP response with status 200 OK if successful, 400 Bad Request if validation fails.</returns>
        [Function("ReceiveOrder")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")]
            HttpRequestData req)
        {
            _logger.LogInformation("Received HTTP POST request at {Timestamp}", DateTime.UtcNow);

            try
            {
                // Read and deserialize the request body
                var requestBody = await req.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    _logger.LogWarning("Request body is empty");
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Request body cannot be empty");
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                OrderRequest? orderRequest = null;
                try
                {
                    orderRequest = JsonSerializer.Deserialize<OrderRequest>(requestBody, options);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize request body");
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, $"Invalid JSON format: {ex.Message}");
                }

                if (orderRequest == null)
                {
                    _logger.LogWarning("Deserialized order request is null");
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Order request cannot be empty");
                }

                // Set OrderDate if not provided
                if (orderRequest.OrderDate == null)
                {
                    orderRequest.OrderDate = DateTime.UtcNow;
                }

                // Validate the order request model
                var validationContext = new ValidationContext(orderRequest);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(orderRequest, validationContext, validationResults, validateAllProperties: true);

                if (!isValid)
                {
                    var errorMessages = string.Join("; ", validationResults.Select(v => v.ErrorMessage));
                    _logger.LogWarning("OrderRequest validation failed: {Errors}", errorMessages);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, $"Validation failed: {errorMessages}");
                }

                // Process the order
                await _orderProcessor.ProcessAsync(orderRequest, _storageConnectionString);

                // Create success response
                _logger.LogInformation("Order processed successfully. OrderId: {OrderId}", orderRequest.OrderId);
                var successResponse = req.CreateResponse(HttpStatusCode.OK);

                var successContent = new
                {
                    success = true,
                    message = "Order received and queued for processing",
                    orderId = orderRequest.OrderId,
                    timestamp = DateTime.UtcNow.ToString("O")  // Pre-format to string
                };

                var successJson = JsonSerializer.Serialize(successContent);
                var successBytes = Encoding.UTF8.GetBytes(successJson);
                successResponse.Body.Write(successBytes, 0, successBytes.Length);
                return successResponse;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operational error processing order");
                return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Failed to process order: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing order");
                return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An unexpected error occurred while processing the order");
            }
        }

        /// <summary>
        /// Creates a standardized error response.
        /// </summary>
        /// <param name="req">The original HTTP request.</param>
        /// <param name="statusCode">The HTTP status code for the error response.</param>
        /// <param name="errorMessage">The error message to include in the response.</param>
        /// <returns>An HTTP response with the specified status code and error message.</returns>
        private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string errorMessage)
        {
            var response = req.CreateResponse(statusCode);

            try
            {
                var errorContent = new
                {
                    success = false,
                    error = errorMessage,
                    timestamp = DateTime.UtcNow.ToString("O")  // Pre-format to string to avoid issues
                };

                // Serialize to JSON string first
                var jsonString = JsonSerializer.Serialize(errorContent);
                var jsonBytes = Encoding.UTF8.GetBytes(jsonString);

                // Write directly to Body stream to avoid header issues
                response.Body.Write(jsonBytes, 0, jsonBytes.Length);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating error response");
                // Fallback: write plain error text
                try
                {
                    var plainText = $"{{\"success\":false,\"error\":\"{EscapeJson(errorMessage)}\"}}";
                    var plainBytes = Encoding.UTF8.GetBytes(plainText);
                    response.Body.Write(plainBytes, 0, plainBytes.Length);
                }
                catch { }

                return response;
            }
        }

        /// <summary>
        /// Escapes a string for JSON embedding.
        /// </summary>
        private static string EscapeJson(string text)
        {
            return text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    }
}
