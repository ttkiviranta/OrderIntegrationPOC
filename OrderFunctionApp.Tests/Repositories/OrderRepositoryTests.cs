using Microsoft.EntityFrameworkCore;
using OrderFunctionApp.Data;
using OrderFunctionApp.Data.Repositories;
using OrderFunctionApp.Models;

namespace OrderFunctionApp.Tests.Repositories
{
    /// <summary>
    /// Unit tests for OrderRepository using xUnit.
    /// Tests CRUD operations and query methods.
    /// </summary>
    public class OrderRepositoryTests
    {
        private OrderIntegrationContext CreateTestContext()
        {
            var options = new DbContextOptionsBuilder<OrderIntegrationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new OrderIntegrationContext(options);
        }

        [Fact]
        public async Task AddAsync_ValidOrder_ShouldSucceed()
        {
            // Arrange
            using var context = CreateTestContext();
            var repository = new OrderRepository(context);
            var order = new Order
            {
                OrderId = "ORD-001",
                CustomerId = "CUST-001",
                Total = 100m,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            // Act
            await repository.AddAsync(order);
            await repository.SaveChangesAsync();

            // Assert
            var saved = await context.Orders.FirstOrDefaultAsync(o => o.OrderId == "ORD-001");
            Assert.NotNull(saved);
            Assert.Equal("ORD-001", saved.OrderId);
            Assert.Equal(OrderStatus.Pending, saved.Status);
        }

        [Fact]
        public async Task GetByOrderIdAsync_ExistingOrder_ShouldReturnOrder()
        {
            // Arrange
            using var context = CreateTestContext();
            var repository = new OrderRepository(context);
            var order = new Order
            {
                OrderId = "ORD-002",
                CustomerId = "CUST-002",
                Total = 200m,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Processing
            };
            await repository.AddAsync(order);
            await repository.SaveChangesAsync();

            // Act
            var result = await repository.GetByOrderIdAsync("ORD-002");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ORD-002", result.OrderId);
            Assert.Equal(OrderStatus.Processing, result.Status);
        }

        [Fact]
        public async Task GetByOrderIdAsync_NonExistingOrder_ShouldReturnNull()
        {
            // Arrange
            using var context = CreateTestContext();
            var repository = new OrderRepository(context);

            // Act
            var result = await repository.GetByOrderIdAsync("ORD-999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByStatusAsync_OrdersWithStatus_ShouldReturnMatching()
        {
            // Arrange
            using var context = CreateTestContext();
            var repository = new OrderRepository(context);

            var order1 = new Order
            {
                OrderId = "ORD-003",
                CustomerId = "CUST-001",
                Total = 100m,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Completed
            };

            var order2 = new Order
            {
                OrderId = "ORD-004",
                CustomerId = "CUST-002",
                Total = 200m,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            await repository.AddAsync(order1);
            await repository.AddAsync(order2);
            await repository.SaveChangesAsync();

            // Act
            var result = await repository.GetByStatusAsync(OrderStatus.Completed);

            // Assert
            Assert.Single(result);
            Assert.Equal("ORD-003", result.First().OrderId);
        }

        [Fact]
        public async Task UpdateAsync_ExistingOrder_ShouldUpdateSuccessfully()
        {
            // Arrange
            using var context = CreateTestContext();
            var repository = new OrderRepository(context);

            var order = new Order
            {
                OrderId = "ORD-005",
                CustomerId = "CUST-001",
                Total = 100m,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            await repository.AddAsync(order);
            await repository.SaveChangesAsync();

            // Act
            var existingOrder = await repository.GetByOrderIdAsync("ORD-005");
            Assert.NotNull(existingOrder);

            existingOrder.Status = OrderStatus.Completed;
            existingOrder.UpdatedAt = DateTime.UtcNow;
            await repository.UpdateAsync(existingOrder);
            await repository.SaveChangesAsync();

            // Assert
            var updated = await repository.GetByOrderIdAsync("ORD-005");
            Assert.NotNull(updated);
            Assert.Equal(OrderStatus.Completed, updated.Status);
            Assert.NotNull(updated.UpdatedAt);
        }

        [Fact]
        public async Task GetByCustomerIdAsync_MultipleOrders_ShouldReturnAll()
        {
            // Arrange
            using var context = CreateTestContext();
            var repository = new OrderRepository(context);

            const string customerId = "CUST-100";

            var order1 = new Order
            {
                OrderId = "ORD-006",
                CustomerId = customerId,
                Total = 100m,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            var order2 = new Order
            {
                OrderId = "ORD-007",
                CustomerId = customerId,
                Total = 200m,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Processing
            };

            var order3 = new Order
            {
                OrderId = "ORD-008",
                CustomerId = "CUST-200",
                Total = 150m,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            await repository.AddAsync(order1);
            await repository.AddAsync(order2);
            await repository.AddAsync(order3);
            await repository.SaveChangesAsync();

            // Act
            var result = await repository.GetByCustomerIdAsync(customerId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, o => Assert.Equal(customerId, o.CustomerId));
        }

        [Fact]
        public async Task GetByDateRangeAsync_OrdersInRange_ShouldReturnMatching()
        {
            // Arrange
            using var context = CreateTestContext();
            var repository = new OrderRepository(context);

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);

            var order1 = new Order
            {
                OrderId = "ORD-009",
                CustomerId = "CUST-001",
                Total = 100m,
                OrderDate = yesterday,
                Status = OrderStatus.Pending
            };

            var order2 = new Order
            {
                OrderId = "ORD-010",
                CustomerId = "CUST-001",
                Total = 200m,
                OrderDate = today,
                Status = OrderStatus.Pending
            };

            await repository.AddAsync(order1);
            await repository.AddAsync(order2);
            await repository.SaveChangesAsync();

            // Act
            var result = await repository.GetByDateRangeAsync(yesterday, today);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task AddAsync_NullOrder_ShouldThrow()
        {
            // Arrange
            using var context = CreateTestContext();
            var repository = new OrderRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null!));
        }
    }
}
