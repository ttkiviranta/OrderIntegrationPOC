using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderFunctionApp.Models;
using OrderFunctionApp.Services;

namespace OrderFunctionApp.Functions
{
    /// <summary>
    /// ProcessOrder Azure Function handles incoming order processing requests.
    /// It receives order data via HTTP POST, validates it, and returns appropriate responses.
    /// This function is triggered by the Azure Logic App when a message arrives in the Service Bus queue.
    /// </summary>
    public class ProcessOrder
    {
        private readonly OrderValidator _orderValidator;
        private readonly ILogger<ProcessOrder> _logger;

        public ProcessOrder(OrderValidator orderValidator, ILogger<ProcessOrder> logger)
        {
            _orderValidator = orderValidator;
            _logger = logger;
        }

        /// <summary>
        /// Main function handler for processing orders.
        /// </summary>
        /// <param name="req">The HTTP request containing order data</param>
        /// <returns>HTTP response with status 200 (OK) for successful processing or 400 (Bad Request) for validation errors</returns>
        [Function("ProcessOrder")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders/process")] 
            HttpRequestData req)
        {
            _logger.LogInformation("ProcessOrder function triggered. Request received at {Timestamp}", DateTime.UtcNow);

            try
            {
                // Read the request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation("Request body: {RequestBody}", requestBody);

                // Deserialize the JSON payload
                OrderRequest? order = null;
                try
                {
                    order = JsonConvert.DeserializeObject<OrderRequest>(requestBody);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize JSON request body.");
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 
                        "Invalid JSON format. Please check your request body.");
                }

                // Validate the order
                var validationResult = _orderValidator.ValidateOrder(order);

                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Order validation failed: {ErrorMessage}", validationResult.ErrorMessage);
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, validationResult.ErrorMessage);
                }

                // Order validation passed - process the order
                _logger.LogInformation("Order {OrderId} validation passed. Processing initiated.", order.OrderId);

                // Create success response
                var successResponse = new
                {
                    status = "success",
                    message = "Order processed successfully",
                    orderId = order.OrderId,
                    customerId = order.CustomerId,
                    total = order.Total,
                    processedAt = DateTime.UtcNow,
                    environment = "Local Development" 
                };

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteAsJsonAsync(successResponse);

                _logger.LogInformation("Order {OrderId} processed successfully. Response sent.", order.OrderId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in ProcessOrder function: {ExceptionMessage}", ex.Message);
                return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, 
                    "An unexpected error occurred while processing the order. Please contact support.");
            }
        }

        /// <summary>
        /// Helper method to create error responses.
        /// </summary>
        /// <param name="req">The HTTP request</param>
        /// <param name="statusCode">HTTP status code for the error</param>
        /// <param name="errorMessage">The error message to return</param>
        /// <returns>HTTP response with error information</returns>
        private async Task<HttpResponseData> CreateErrorResponse(
            HttpRequestData req, 
            HttpStatusCode statusCode, 
            string errorMessage)
        {
            var errorResponse = new
            {
                status = "error",
                message = errorMessage,
                timestamp = DateTime.UtcNow,
                environment = "Local Development"
            };

            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteAsJsonAsync(errorResponse);
            return response;
        }
    }
}
