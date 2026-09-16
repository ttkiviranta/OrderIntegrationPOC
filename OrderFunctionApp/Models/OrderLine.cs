namespace OrderFunctionApp.Models
{
    /// <summary>
    /// Represents an Order Line item, which is a line item within an Order.
    /// Each order can have multiple order lines representing individual products or services.
    /// </summary>
    public class OrderLine
    {
        /// <summary>
        /// Gets or sets the unique database identifier for the order line.
        /// This is the primary key (auto-generated).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the foreign key to the associated Order.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the line item number or sequence within the order.
        /// Typically starts at 1.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the product or service identifier (SKU, part number).
        /// </summary>
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the product or service description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the quantity ordered.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit of measurement (e.g., "pcs", "kg", "hours").
        /// </summary>
        public string Unit { get; set; } = "pcs";

        /// <summary>
        /// Gets or sets the unit price.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the total price for this line (Quantity * UnitPrice).
        /// This is a calculated field but stored for performance reasons.
        /// </summary>
        public decimal LineTotal { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage applied to this line (0-100).
        /// </summary>
        public decimal? DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets any additional notes or comments for this line.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the order line was created.
        /// Defaults to the current UTC time.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for Entity Framework relationship
        /// <summary>
        /// Gets or sets the associated Order for this order line.
        /// </summary>
        public Order? Order { get; set; }
    }
}
