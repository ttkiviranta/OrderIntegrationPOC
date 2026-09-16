namespace OrderFunctionApp.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for Order entity used in API responses and internal operations.
    /// This DTO represents the internal view of an Order without exposing database entities.
    /// </summary>
    public class OrderDTO
    {
        /// <summary>
        /// Gets or sets the unique database identifier for the order.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the business order identifier.
        /// </summary>
        public string? OrderId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        public string? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the total order amount.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Gets or sets the order description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the date when the order was placed.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the current status of the order.
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the order was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the order was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the collection of order lines.
        /// </summary>
        public List<OrderLineDTO> OrderLines { get; set; } = new List<OrderLineDTO>();

        /// <summary>
        /// Gets or sets the associated customer information.
        /// </summary>
        public CustomerDTO? Customer { get; set; }
    }
}
