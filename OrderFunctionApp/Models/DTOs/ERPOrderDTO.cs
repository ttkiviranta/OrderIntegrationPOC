namespace OrderFunctionApp.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for order data received from external ERP systems.
    /// This DTO represents the format that external systems use to submit orders.
    /// </summary>
    public class ERPOrderDTO
    {
        /// <summary>
        /// Gets or sets the external/ERP system's order identifier.
        /// This is mapped to the Order.OrderId in the internal system.
        /// </summary>
        public string? ExternalOrderId { get; set; }

        /// <summary>
        /// Gets or sets the external customer identifier from the ERP system.
        /// This is mapped to Order.CustomerId in the internal system.
        /// </summary>
        public string? ExternalCustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer name from the ERP system.
        /// Used to create or update the Customer entity.
        /// </summary>
        public string? CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the customer email address from the ERP system.
        /// </summary>
        public string? CustomerEmail { get; set; }

        /// <summary>
        /// Gets or sets the total order amount from the ERP system.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Gets or sets the order description from the ERP system.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the order date from the ERP system.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the collection of order line items from the ERP system.
        /// </summary>
        public List<ERPOrderLineDTO> LineItems { get; set; } = new List<ERPOrderLineDTO>();

        /// <summary>
        /// Gets or sets optional metadata from the ERP system.
        /// This can be used for audit trails or system-specific information.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
