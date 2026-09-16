using OrderFunctionApp.Models;

namespace OrderFunctionApp.Data.Repositories
{
    /// <summary>
    /// Interface defining data access operations for Customer entities.
    /// </summary>
    public interface ICustomerRepository
    {
        /// <summary>
        /// Adds a new customer to the repository.
        /// </summary>
        /// <param name="customer">The customer to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(Customer customer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing customer in the repository.
        /// </summary>
        /// <param name="customer">The customer to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a customer by its database ID.
        /// </summary>
        /// <param name="id">The customer database ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The customer if found; otherwise null.</returns>
        Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a customer by its business customer identifier.
        /// </summary>
        /// <param name="customerId">The business customer identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The customer if found; otherwise null.</returns>
        Task<Customer?> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all customers.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All customers in the repository.</returns>
        Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all active customers.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All active customers in the repository.</returns>
        Task<IEnumerable<Customer>> GetActiveAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves changes to the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
