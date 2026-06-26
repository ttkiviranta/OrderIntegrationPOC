using System;
using System.ComponentModel.DataAnnotations;

namespace OrderFunctionApp.Models
{
    /// <summary>
    /// Represents an incoming order request from the ERP system.
    /// This model is used to deserialize JSON payloads from Azure Service Bus.
    /// </summary>
    public class OrderRequest
    {
        /// <summary>
        /// Gets or sets the unique order identifier.
        /// </summary>
        [Required(ErrorMessage = "OrderId is required")]
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        [Required(ErrorMessage = "CustomerId is required")]
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the total order amount.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Total must be greater than 0")]
        public decimal Total { get; set; }

        /// <summary>
        /// Gets or sets an optional order description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets an optional order date (defaults to current UTC time if not provided).
        /// </summary>
        public DateTime? OrderDate { get; set; }
    }
}
