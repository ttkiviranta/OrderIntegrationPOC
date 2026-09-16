namespace OrderFunctionApp.Models
{
    /// <summary>
    /// Represents an Order entity stored in the database.
    /// This class is used by Entity Framework Core for database operations.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Gets or sets the unique database identifier for the order.
        /// This is the primary key (auto-generated).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the business order identifier (e.g., ORD-2024-001).
        /// This is a UNIQUE constraint to prevent duplicate orders.
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer identifier.
        /// This is indexed for fast customer-based queries.
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total order amount.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Gets or sets an optional order description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the date when the order was placed.
        /// This is indexed for date range queries.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the order was inserted into the database.
        /// This serves as an audit trail and defaults to the current UTC time.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp when the order was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the current status of the order.
        /// Defaults to Pending.
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>
        /// Gets or sets the foreign key to the associated Customer.
        /// This provides a typed relationship to the Customer entity.
        /// </summary>
        public int? CustomerDatabaseId { get; set; }

        // Navigation properties for Entity Framework relationships
        /// <summary>
        /// Gets or sets the collection of order lines associated with this order.
        /// </summary>
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();

        /// <summary>
        /// Gets or sets the associated Customer for this order.
        /// </summary>
        public Customer? Customer { get; set; }
    }
}
