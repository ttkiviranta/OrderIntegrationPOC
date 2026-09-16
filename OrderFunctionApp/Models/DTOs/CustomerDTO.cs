namespace OrderFunctionApp.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for Customer entity used in API responses.
    /// </summary>
    public class CustomerDTO
    {
        /// <summary>
        /// Gets or sets the unique database identifier for the customer.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the business customer identifier.
        /// </summary>
        public string? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer's display name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the customer's email address.
        /// </summary>
        public string? Email { get; set; }

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
        /// Gets or sets whether the customer is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the customer record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the customer record was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
