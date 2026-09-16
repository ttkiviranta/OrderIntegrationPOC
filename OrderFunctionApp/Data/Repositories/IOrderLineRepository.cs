using OrderFunctionApp.Models;

namespace OrderFunctionApp.Data.Repositories
{
    /// <summary>
    /// Interface defining data access operations for OrderLine entities.
    /// </summary>
    public interface IOrderLineRepository
    {
        /// <summary>
        /// Adds a new order line to the repository.
        /// </summary>
        /// <param name="orderLine">The order line to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(OrderLine orderLine, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing order line in the repository.
        /// </summary>
        /// <param name="orderLine">The order line to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(OrderLine orderLine, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an order line by its database ID.
        /// </summary>
        /// <param name="id">The order line database ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The order line if found; otherwise null.</returns>
        Task<OrderLine?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all order lines for a specific order.
        /// </summary>
        /// <param name="orderId">The order database ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of order lines for the order.</returns>
        Task<IEnumerable<OrderLine>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds multiple order lines to the repository.
        /// </summary>
        /// <param name="orderLines">The order lines to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddRangeAsync(IEnumerable<OrderLine> orderLines, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves changes to the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
