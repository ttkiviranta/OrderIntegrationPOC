namespace OrderFunctionApp.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for order line items received from external ERP systems.
    /// </summary>
    public class ERPOrderLineDTO
    {
        /// <summary>
        /// Gets or sets the line item number or sequence.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the product or service identifier from the ERP system.
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
        /// Gets or sets the unit of measurement (e.g., "pcs", "kg", "hours").
        /// </summary>
        public string Unit { get; set; } = "pcs";

        /// <summary>
        /// Gets or sets the unit price.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the total price for this line (Quantity * UnitPrice).
        /// </summary>
        public decimal LineTotal { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage applied to this line (0-100).
        /// </summary>
        public decimal? DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets any additional notes for this line.
        /// </summary>
        public string? Notes { get; set; }
    }
}
