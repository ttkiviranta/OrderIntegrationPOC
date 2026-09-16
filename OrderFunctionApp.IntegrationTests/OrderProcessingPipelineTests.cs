using Microsoft.EntityFrameworkCore;
using OrderFunctionApp.Data;
using OrderFunctionApp.Data.Repositories;
using OrderFunctionApp.Models;
using OrderFunctionApp.Models.DTOs;
using OrderFunctionApp.IntegrationTests.Fixtures;
using AutoMapper;
using OrderFunctionApp.Models.Mappings;

namespace OrderFunctionApp.IntegrationTests
{
    /// <summary>
    /// Integration tests for the complete order processing pipeline.
    /// Tests the flow from ERP order DTO to database persistence.
    /// </summary>
    public class OrderProcessingPipelineTests
    {
        private OrderIntegrationContext CreateTestContext()
        {
            var options = new DbContextOptionsBuilder<OrderIntegrationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new OrderIntegrationContext(options);
        }

        private IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderMappingProfile>());
            return config.CreateMapper();
        }

        [Fact]
        public async Task ProcessOrder_ValidERPOrder_ShouldPersistSuccessfully()
        {
            // Arrange
            using var context = CreateTestContext();
            var orderRepository = new OrderRepository(context);
            var customerRepository = new CustomerRepository(context);
            var mapper = GetMapper();

            var erpOrder = MockOrderData.CreateValidTestOrder();

            // Act - Map ERP order to internal entities
            var customer = mapper.Map<Customer>(erpOrder);
            await customerRepository.AddAsync(customer);
            await customerRepository.SaveChangesAsync();

            var internalOrder = mapper.Map<Order>(erpOrder);
            internalOrder.CustomerDatabaseId = customer.Id;

            var orderLines = erpOrder.LineItems
                .Select(li => mapper.Map<OrderLine>(li))
                .ToList();

            internalOrder.OrderLines = orderLines;

            await orderRepository.AddAsync(internalOrder);
            await orderRepository.SaveChangesAsync();

            // Assert
            var savedOrder = await orderRepository.GetByOrderIdAsync(erpOrder.ExternalOrderId!);
            Assert.NotNull(savedOrder);
            Assert.Equal(erpOrder.ExternalOrderId, savedOrder.OrderId);
            Assert.Equal(erpOrder.Total, savedOrder.Total);
            Assert.Equal(OrderStatus.Pending, savedOrder.Status);
            Assert.Equal(2, savedOrder.OrderLines.Count);

            var savedCustomer = await customerRepository.GetByCustomerIdAsync(erpOrder.ExternalCustomerId!);
            Assert.NotNull(savedCustomer);
            Assert.Equal(erpOrder.CustomerName, savedCustomer.Name);
            Assert.Equal(erpOrder.CustomerEmail, savedCustomer.Email);
        }

        [Fact]
        public async Task ProcessOrder_MultipleOrdersForSameCustomer_ShouldMaintainRelationship()
        {
            // Arrange
            using var context = CreateTestContext();
            var orderRepository = new OrderRepository(context);
            var customerRepository = new CustomerRepository(context);
            var mapper = GetMapper();

            var customerId = $"CUST-{Guid.NewGuid().ToString().Substring(0, 8)}";
            var erpOrder1 = MockOrderData.CreateValidTestOrder();
            var erpOrder2 = MockOrderData.CreateValidTestOrder();

            erpOrder1.ExternalCustomerId = customerId;
            erpOrder2.ExternalCustomerId = customerId;
            erpOrder1.ExternalOrderId = "ORD-001";
            erpOrder2.ExternalOrderId = "ORD-002";

            // Act
            var customer = mapper.Map<Customer>(erpOrder1);
            await customerRepository.AddAsync(customer);
            await customerRepository.SaveChangesAsync();

            var order1 = mapper.Map<Order>(erpOrder1);
            order1.CustomerDatabaseId = customer.Id;
            await orderRepository.AddAsync(order1);

            var order2 = mapper.Map<Order>(erpOrder2);
            order2.CustomerDatabaseId = customer.Id;
            await orderRepository.AddAsync(order2);

            await orderRepository.SaveChangesAsync();

            // Assert
            var orders = await orderRepository.GetByCustomerIdAsync(customerId);
            Assert.Equal(2, orders.Count());

            var loadedCustomer = await customerRepository.GetByCustomerIdAsync(customerId);
            Assert.NotNull(loadedCustomer);
            Assert.Equal(2, loadedCustomer.Orders.Count);
        }

        [Fact]
        public async Task ProcessOrder_UpdateOrderStatus_ShouldReflectChanges()
        {
            // Arrange
            using var context = CreateTestContext();
            var orderRepository = new OrderRepository(context);
            var mapper = GetMapper();

            var erpOrder = MockOrderData.CreateMinimalTestOrder();
            var internalOrder = mapper.Map<Order>(erpOrder);

            await orderRepository.AddAsync(internalOrder);
            await orderRepository.SaveChangesAsync();

            // Act - Update status
            var savedOrder = await orderRepository.GetByOrderIdAsync(erpOrder.ExternalOrderId!);
            Assert.NotNull(savedOrder);

            savedOrder.Status = OrderStatus.Processing;
            savedOrder.UpdatedAt = DateTime.UtcNow;
            await orderRepository.UpdateAsync(savedOrder);
            await orderRepository.SaveChangesAsync();

            // Assert
            var updatedOrder = await orderRepository.GetByOrderIdAsync(erpOrder.ExternalOrderId!);
            Assert.Equal(OrderStatus.Processing, updatedOrder!.Status);
            Assert.NotNull(updatedOrder.UpdatedAt);
        }

        [Fact]
        public async Task ProcessOrder_BatchOperations_ShouldHandleMultipleOrders()
        {
            // Arrange
            using var context = CreateTestContext();
            var orderRepository = new OrderRepository(context);
            var customerRepository = new CustomerRepository(context);
            var mapper = GetMapper();

            var batchOrders = MockOrderData.CreateBatchTestOrders(5);

            // Act - Process all orders
            foreach (var erpOrder in batchOrders)
            {
                var customer = mapper.Map<Customer>(erpOrder);
                await customerRepository.AddAsync(customer);
                await customerRepository.SaveChangesAsync();

                var internalOrder = mapper.Map<Order>(erpOrder);
                internalOrder.CustomerDatabaseId = customer.Id;

                var orderLines = erpOrder.LineItems
                    .Select(li => mapper.Map<OrderLine>(li))
                    .ToList();

                internalOrder.OrderLines = orderLines;

                await orderRepository.AddAsync(internalOrder);
            }

            await orderRepository.SaveChangesAsync();

            // Assert
            var allOrders = await orderRepository.GetAllAsync();
            Assert.Equal(5, allOrders.Count());

            var allCustomers = await customerRepository.GetAllAsync();
            Assert.Equal(5, allCustomers.Count());
        }

        [Fact]
        public async Task ProcessOrder_Idempotency_DuplicateOrderIdShouldNotCreateDuplicate()
        {
            // Arrange
            using var context = CreateTestContext();
            var orderRepository = new OrderRepository(context);
            var customerRepository = new CustomerRepository(context);
            var mapper = GetMapper();

            var erpOrder = MockOrderData.CreateValidTestOrder("IDEMPOTENT-ORD-001");

            // Act - Process same order twice
            for (int i = 0; i < 2; i++)
            {
                var existingOrder = await orderRepository.GetByOrderIdAsync(erpOrder.ExternalOrderId!);
                if (existingOrder == null)
                {
                    var customer = mapper.Map<Customer>(erpOrder);
                    await customerRepository.AddAsync(customer);
                    await customerRepository.SaveChangesAsync();

                    var internalOrder = mapper.Map<Order>(erpOrder);
                    internalOrder.CustomerDatabaseId = customer.Id;
                    await orderRepository.AddAsync(internalOrder);
                    await orderRepository.SaveChangesAsync();
                }
            }

            // Assert
            var allOrders = await orderRepository.GetAllAsync();
            Assert.Single(allOrders);
            Assert.Equal("IDEMPOTENT-ORD-001", allOrders.First().OrderId);
        }

        [Fact]
        public async Task ProcessOrder_DateRangeQuery_ShouldFilterCorrectly()
        {
            // Arrange
            using var context = CreateTestContext();
            var orderRepository = new OrderRepository(context);
            var customerRepository = new CustomerRepository(context);
            var mapper = GetMapper();

            var today = DateTime.UtcNow.Date;

            var erpOrder1 = MockOrderData.CreateValidTestOrder();
            erpOrder1.OrderDate = today.AddDays(-5);

            var erpOrder2 = MockOrderData.CreateValidTestOrder();
            erpOrder2.OrderDate = today;

            var erpOrder3 = MockOrderData.CreateValidTestOrder();
            erpOrder3.OrderDate = today.AddDays(5);  // Future date

            // Act
            foreach (var erpOrder in new[] { erpOrder1, erpOrder2, erpOrder3 })
            {
                var customer = mapper.Map<Customer>(erpOrder);
                await customerRepository.AddAsync(customer);
                await customerRepository.SaveChangesAsync();

                var internalOrder = mapper.Map<Order>(erpOrder);
                internalOrder.CustomerDatabaseId = customer.Id;
                internalOrder.OrderDate = erpOrder.OrderDate;
                await orderRepository.AddAsync(internalOrder);
            }

            await orderRepository.SaveChangesAsync();

            // Assert
            var ordersInRange = await orderRepository.GetByDateRangeAsync(today.AddDays(-10), today);
            Assert.Equal(2, ordersInRange.Count());
        }
    }
}
