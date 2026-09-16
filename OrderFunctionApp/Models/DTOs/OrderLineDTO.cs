namespace OrderFunctionApp.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for OrderLine entity used in API responses.
    /// </summary>
    public class OrderLineDTO
    {
        /// <summary>
        /// Gets or sets the unique database identifier for the order line.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the line item number.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the product or service identifier.
        /// </summary>
        public string? ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product or service description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the quantity ordered.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit of measurement.
        /// </summary>
        public string Unit { get; set; } = "pcs";

        /// <summary>
        /// Gets or sets the unit price.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the total price for this line.
        /// </summary>
        public decimal LineTotal { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage.
        /// </summary>
        public decimal? DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets any additional notes for this line.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the order line was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
