using Microsoft.EntityFrameworkCore;
using OrderFunctionApp.Models;

namespace OrderFunctionApp.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the IOrderLineRepository interface.
    /// Provides data access operations for OrderLine entities.
    /// </summary>
    public class OrderLineRepository : IOrderLineRepository
    {
        private readonly OrderIntegrationContext _context;

        public OrderLineRepository(OrderIntegrationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(OrderLine orderLine, CancellationToken cancellationToken = default)
        {
            if (orderLine == null)
                throw new ArgumentNullException(nameof(orderLine));

            await _context.OrderLines.AddAsync(orderLine, cancellationToken);
        }

        public async Task UpdateAsync(OrderLine orderLine, CancellationToken cancellationToken = default)
        {
            if (orderLine == null)
                throw new ArgumentNullException(nameof(orderLine));

            _context.OrderLines.Update(orderLine);
            await Task.CompletedTask;
        }

        public async Task<OrderLine?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.OrderLines
                .Include(ol => ol.Order)
                .FirstOrDefaultAsync(ol => ol.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<OrderLine>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return await _context.OrderLines
                .Where(ol => ol.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<OrderLine> orderLines, CancellationToken cancellationToken = default)
        {
            if (orderLines == null)
                throw new ArgumentNullException(nameof(orderLines));

            await _context.OrderLines.AddRangeAsync(orderLines, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
