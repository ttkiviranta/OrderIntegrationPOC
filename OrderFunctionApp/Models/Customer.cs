namespace OrderFunctionApp.Models
{
    /// <summary>
    /// Represents a Customer entity in the system.
    /// Customers can have multiple orders and order lines.
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Gets or sets the unique database identifier for the customer.
        /// This is the primary key (auto-generated).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the business customer identifier (e.g., CUST-2024-001).
        /// This is a UNIQUE constraint to prevent duplicate customers.
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer's display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer's phone number.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Gets or sets the customer's business registration number or tax ID.
        /// </summary>
        public string? RegistrationNumber { get; set; }

        /// <summary>
        /// Gets or sets the customer's billing address.
        /// </summary>
        public string? BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets the customer's shipping address.
        /// </summary>
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the customer record was created.
        /// Defaults to the current UTC time.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp when the customer record was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets whether the customer is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties for Entity Framework relationships
        /// <summary>
        /// Gets or sets the collection of orders associated with this customer.
        /// </summary>
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
