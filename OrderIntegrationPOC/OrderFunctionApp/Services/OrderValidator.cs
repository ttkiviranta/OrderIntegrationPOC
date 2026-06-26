using Microsoft.Extensions.Logging;
using OrderFunctionApp.Models;

namespace OrderFunctionApp.Services
{
    /// <summary>
    /// OrderValidator service handles the validation of incoming order requests.
    /// It checks for required fields and validates business rules.
    /// </summary>
    public class OrderValidator
    {
        private readonly ILogger<OrderValidator> _logger;

        public OrderValidator(ILogger<OrderValidator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates the order request data.
        /// </summary>
        /// <param name="order">The order request to validate</param>
        /// <returns>OrderValidationResult containing validation status and any error messages</returns>
        public OrderValidationResult ValidateOrder(OrderRequest? order)
        {
            var result = new OrderValidationResult();

            // Check if order object is null
            if (order == null)
            {
                result.IsValid = false;
                result.ErrorMessage = "Order request cannot be null.";
                _logger.LogWarning("Validation failed: Order request is null.");
                return result;
            }

            // Validate required field: orderId
            if (string.IsNullOrWhiteSpace(order.OrderId))
            {
                result.IsValid = false;
                result.ErrorMessage = "Field 'orderId' is required and cannot be empty.";
                _logger.LogWarning("Validation failed: OrderId is missing or empty.");
                return result;
            }

            // Validate required field: customerId
            if (string.IsNullOrWhiteSpace(order.CustomerId))
            {
                result.IsValid = false;
                result.ErrorMessage = "Field 'customerId' is required and cannot be empty.";
                _logger.LogWarning("Validation failed: CustomerId is missing or empty.");
                return result;
            }

            // Validate total amount
            if (order.Total <= 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "Field 'total' must be greater than zero.";
                _logger.LogWarning($"Validation failed: Total amount is invalid ({order.Total})."); 
                return result;
            }

            // All validations passed
            result.IsValid = true;
            result.ProcessedOrderId = order.OrderId;
            _logger.LogInformation($"Order {order.OrderId} passed validation. Processing for customer {order.CustomerId} with total {order.Total:C}.");

            return result;
        }
    }
}
