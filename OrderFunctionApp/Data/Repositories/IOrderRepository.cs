using OrderFunctionApp.Models;

namespace OrderFunctionApp.Data.Repositories
{
    /// <summary>
    /// Interface defining data access operations for Order entities.
    /// Abstracts the data persistence layer from business logic.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Adds a new order to the repository.
        /// </summary>
        /// <param name="order">The order to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(Order order, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing order in the repository.
        /// </summary>
        /// <param name="order">The order to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(Order order, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an order by its database ID.
        /// </summary>
        /// <param name="id">The order database ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The order if found; otherwise null.</returns>
        Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an order by its business order identifier.
        /// </summary>
        /// <param name="orderId">The business order identifier (e.g., ORD-2024-001).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The order if found; otherwise null.</returns>
        Task<Order?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all orders for a specific customer.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of orders for the customer.</returns>
        Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all orders with a specific status.
        /// </summary>
        /// <param name="status">The order status.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of orders with the specified status.</returns>
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all orders created within a date range.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of orders created in the date range.</returns>
        Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all orders.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All orders in the repository.</returns>
        Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves changes to the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
