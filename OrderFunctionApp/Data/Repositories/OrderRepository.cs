using Microsoft.EntityFrameworkCore;
using OrderFunctionApp.Models;

namespace OrderFunctionApp.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the IOrderRepository interface.
    /// Provides data access operations for Order entities.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderIntegrationContext _context;

        public OrderRepository(OrderIntegrationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            await _context.Orders.AddAsync(order, cancellationToken);
        }

        public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _context.Orders.Update(order);
            await Task.CompletedTask;
        }

        public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.OrderLines)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<Order?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("Order ID cannot be null or empty.", nameof(orderId));

            return await _context.Orders
                .Include(o => o.OrderLines)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
        }

        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer ID cannot be null or empty.", nameof(customerId));

            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.OrderLines)
                .Include(o => o.Customer)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Where(o => o.Status == status)
                .Include(o => o.OrderLines)
                .Include(o => o.Customer)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .Include(o => o.OrderLines)
                .Include(o => o.Customer)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.OrderLines)
                .Include(o => o.Customer)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
