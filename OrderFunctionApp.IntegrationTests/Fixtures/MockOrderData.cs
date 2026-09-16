using OrderFunctionApp.Models.DTOs;

namespace OrderFunctionApp.IntegrationTests.Fixtures
{
    /// <summary>
    /// Provides mock/test data for integration tests.
    /// </summary>
    public static class MockOrderData
    {
        /// <summary>
        /// Creates a valid test ERP order DTO.
        /// </summary>
        public static ERPOrderDTO CreateValidTestOrder(string? orderId = null)
        {
            return new ERPOrderDTO
            {
                ExternalOrderId = orderId ?? $"ORD-{Guid.NewGuid().ToString().Substring(0, 8)}",
                ExternalCustomerId = $"CUST-{Guid.NewGuid().ToString().Substring(0, 8)}",
                CustomerName = "Test Customer Company",
                CustomerEmail = "test.customer@example.com",
                Total = 10500.75m,
                Description = "Test order for integration testing",
                OrderDate = DateTime.UtcNow.AddDays(-5),
                LineItems = new List<ERPOrderLineDTO>
                {
                    new ERPOrderLineDTO
                    {
                        LineNumber = 1,
                        ProductId = "PROD-001",
                        Description = "Laptop Computer",
                        Quantity = 2,
                        Unit = "pcs",
                        UnitPrice = 5000m,
                        LineTotal = 10000m,
                        DiscountPercentage = 0.5m,
                        Notes = "High-performance model"
                    },
                    new ERPOrderLineDTO
                    {
                        LineNumber = 2,
                        ProductId = "SERV-001",
                        Description = "Installation and Setup Service",
                        Quantity = 1,
                        Unit = "hours",
                        UnitPrice = 500.75m,
                        LineTotal = 500.75m,
                        Notes = "Professional installation"
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "source", "erp-system-test" },
                    { "department", "sales" },
                    { "priority", "high" }
                }
            };
        }

        /// <summary>
        /// Creates a test order with minimal data.
        /// </summary>
        public static ERPOrderDTO CreateMinimalTestOrder()
        {
            return new ERPOrderDTO
            {
                ExternalOrderId = $"MIN-{Guid.NewGuid().ToString().Substring(0, 8)}",
                ExternalCustomerId = "CUST-MIN-001",
                CustomerName = "Minimal Customer",
                CustomerEmail = "minimal@test.com",
                Total = 100m,
                OrderDate = DateTime.UtcNow,
                LineItems = new List<ERPOrderLineDTO>
                {
                    new ERPOrderLineDTO
                    {
                        LineNumber = 1,
                        ProductId = "PROD-MIN",
                        Description = "Minimal Product",
                        Quantity = 1,
                        Unit = "pcs",
                        UnitPrice = 100m,
                        LineTotal = 100m
                    }
                }
            };
        }

        /// <summary>
        /// Creates multiple test orders for batch testing.
        /// </summary>
        public static List<ERPOrderDTO> CreateBatchTestOrders(int count)
        {
            var orders = new List<ERPOrderDTO>();

            for (int i = 0; i < count; i++)
            {
                orders.Add(new ERPOrderDTO
                {
                    ExternalOrderId = $"BATCH-{i:D4}",
                    ExternalCustomerId = $"CUST-BATCH-{i:D3}",
                    CustomerName = $"Batch Customer {i}",
                    CustomerEmail = $"customer{i}@batch.test",
                    Total = (i + 1) * 1000m,
                    OrderDate = DateTime.UtcNow.AddDays(-i),
                    LineItems = new List<ERPOrderLineDTO>
                    {
                        new ERPOrderLineDTO
                        {
                            LineNumber = 1,
                            ProductId = $"PROD-BATCH-{i}",
                            Description = $"Batch Product {i}",
                            Quantity = i + 1,
                            Unit = "pcs",
                            UnitPrice = 1000m,
                            LineTotal = (i + 1) * 1000m
                        }
                    }
                });
            }

            return orders;
        }

        /// <summary>
        /// Creates a test Service Bus message (JSON string) from an ERP order.
        /// </summary>
        public static string CreateServiceBusMessage(ERPOrderDTO order)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(order, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
            return json;
        }
    }
}
