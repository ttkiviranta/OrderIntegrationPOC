using OrderFunctionApp.Models.DTOs;
using OrderFunctionApp.Validation;

namespace OrderFunctionApp.Tests.Validation
{
    /// <summary>
    /// Unit tests for ERPOrderDTOValidator using xUnit.
    /// Tests validation rules for ERP order data.
    /// </summary>
    public class ERPOrderDTOValidatorTests
    {
        private readonly ERPOrderDTOValidator _validator;

        public ERPOrderDTOValidatorTests()
        {
            _validator = new ERPOrderDTOValidator();
        }

        [Fact]
        public void Validate_ValidOrder_ShouldSucceed()
        {
            // Arrange
            var order = new ERPOrderDTO
            {
                ExternalOrderId = "ORD-001",
                ExternalCustomerId = "CUST-001",
                CustomerName = "Test Customer",
                CustomerEmail = "test@example.com",
                Total = 100.50m,
                OrderDate = DateTime.UtcNow.AddDays(-1),
                LineItems = new List<ERPOrderLineDTO>
                {
                    new ERPOrderLineDTO
                    {
                        LineNumber = 1,
                        ProductId = "PROD-001",
                        Description = "Test Product",
                        Quantity = 2,
                        Unit = "pcs",
                        UnitPrice = 50.25m,
                        LineTotal = 100.50m
                    }
                }
            };

            // Act
            var result = _validator.Validate(order);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_MissingExternalOrderId_ShouldFail()
        {
            // Arrange
            var order = new ERPOrderDTO
            {
                ExternalOrderId = null,
                ExternalCustomerId = "CUST-001",
                CustomerName = "Test Customer",
                CustomerEmail = "test@example.com",
                Total = 100.50m,
                OrderDate = DateTime.UtcNow
            };

            // Act
            var result = _validator.Validate(order);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ExternalOrderId");
        }

        [Fact]
        public void Validate_InvalidTotalAmount_ShouldFail()
        {
            // Arrange
            var order = new ERPOrderDTO
            {
                ExternalOrderId = "ORD-001",
                ExternalCustomerId = "CUST-001",
                CustomerName = "Test Customer",
                CustomerEmail = "test@example.com",
                Total = -100m,  // Invalid: negative total
                OrderDate = DateTime.UtcNow
            };

            // Act
            var result = _validator.Validate(order);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Total");
        }

        [Fact]
        public void Validate_FutureOrderDate_ShouldFail()
        {
            // Arrange
            var order = new ERPOrderDTO
            {
                ExternalOrderId = "ORD-001",
                ExternalCustomerId = "CUST-001",
                CustomerName = "Test Customer",
                CustomerEmail = "test@example.com",
                Total = 100.50m,
                OrderDate = DateTime.UtcNow.AddDays(1)  // Future date
            };

            // Act
            var result = _validator.Validate(order);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "OrderDate");
        }

        [Fact]
        public void Validate_NoLineItems_ShouldFail()
        {
            // Arrange
            var order = new ERPOrderDTO
            {
                ExternalOrderId = "ORD-001",
                ExternalCustomerId = "CUST-001",
                CustomerName = "Test Customer",
                CustomerEmail = "test@example.com",
                Total = 100.50m,
                OrderDate = DateTime.UtcNow,
                LineItems = new List<ERPOrderLineDTO>()  // Empty
            };

            // Act
            var result = _validator.Validate(order);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "LineItems");
        }

        [Fact]
        public void Validate_InvalidEmail_ShouldFail()
        {
            // Arrange
            var order = new ERPOrderDTO
            {
                ExternalOrderId = "ORD-001",
                ExternalCustomerId = "CUST-001",
                CustomerName = "Test Customer",
                CustomerEmail = "invalid-email",  // Invalid email format
                Total = 100.50m,
                OrderDate = DateTime.UtcNow,
                LineItems = new List<ERPOrderLineDTO>
                {
                    new ERPOrderLineDTO
                    {
                        LineNumber = 1,
                        ProductId = "PROD-001",
                        Description = "Test Product",
                        Quantity = 1,
                        Unit = "pcs",
                        UnitPrice = 100.50m,
                        LineTotal = 100.50m
                    }
                }
            };

            // Act
            var result = _validator.Validate(order);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CustomerEmail");
        }
    }
}
