namespace OrderFunctionApp.Models
{
    /// <summary>
    /// Represents the result of order validation.
    /// Contains validation status and error information if validation fails.
    /// </summary>
    public class OrderValidationResult
    {
        /// <summary>
        /// Indicates whether the order validation passed.
        /// </summary>
        public bool IsValid { get; set; } = false;

        /// <summary>
        /// Error message if validation fails. Null if validation passes.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// The Order ID of the processed order. Set only if validation passes.
        /// </summary>
        public string? ProcessedOrderId { get; set; }
    }
}
