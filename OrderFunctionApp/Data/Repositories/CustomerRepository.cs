using Microsoft.EntityFrameworkCore;
using OrderFunctionApp.Models;

namespace OrderFunctionApp.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the ICustomerRepository interface.
    /// Provides data access operations for Customer entities.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly OrderIntegrationContext _context;

        public CustomerRepository(OrderIntegrationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            await _context.Customers.AddAsync(customer, cancellationToken);
        }

        public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            _context.Customers.Update(customer);
            await Task.CompletedTask;
        }

        public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Customer?> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer ID cannot be null or empty.", nameof(customerId));

            return await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Customer>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Where(c => c.IsActive)
                .Include(c => c.Orders)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
