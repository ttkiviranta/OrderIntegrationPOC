using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using OrderFunctionApp.Data;
using OrderFunctionApp.Data.Repositories;
using OrderFunctionApp.Exceptions;
using OrderFunctionApp.Models;
using OrderFunctionApp.Models.DTOs;
using OrderFunctionApp.Services;

namespace OrderFunctionApp.Functions
{
    /// <summary>
    /// Service Bus triggered Azure Function that ingests orders from the Service Bus queue.
    /// This function receives ERP order messages, validates them, and persists to SQL Database.
    /// Includes comprehensive error handling, retry logic, and poison message handling.
    /// </summary>
    public class ServiceBusOrderIngestion
    {
        private readonly IDbContextFactory<OrderIntegrationContext> _dbContextFactory;
        private readonly IMapper _mapper;
        private readonly RetryPolicy _retryPolicy;
        private readonly PoisonMessageHandler _poisonHandler;

        public ServiceBusOrderIngestion(
            IDbContextFactory<OrderIntegrationContext> dbContextFactory,
            IMapper mapper,
            RetryPolicy retryPolicy,
            PoisonMessageHandler poisonHandler)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
            _poisonHandler = poisonHandler ?? throw new ArgumentNullException(nameof(poisonHandler));
        }

        [Function("ServiceBusOrderIngestion")]
        public async Task Run(
            [ServiceBusTrigger("orders-incoming", Connection = "ServiceBusConnection")] string message,
            FunctionContext context)
        {
            var logger = context.GetLogger("ServiceBusOrderIngestion");
            logger.LogInformation("=== Service Bus Order Ingestion Triggered ===");
            logger.LogInformation("Received message: {Message}", message);

            try
            {
                // Deserialize the ERP order DTO
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var erpOrderDto = JsonSerializer.Deserialize<ERPOrderDTO>(message, options);

                if (erpOrderDto == null)
                {
                    logger.LogError("Failed to deserialize Service Bus message: {Message}", message);
                    throw new OrderValidationException("Order message deserialization failed");
                }

                logger.LogInformation("Processing order from ERP system - External Order ID: {ExternalOrderId}",
                    erpOrderDto.ExternalOrderId);

                // Process the order with retry policy
                await _retryPolicy.ExecuteWithRetryAsync(
                    async cancellationToken =>
                    {
                        await ProcessOrderAsync(erpOrderDto, logger, cancellationToken);
                    },
                    operationName: $"Process order {erpOrderDto.ExternalOrderId}",
                    maxRetries: 3,
                    initialDelayMs: 1000,
                    cancellationToken: CancellationToken.None);

                logger.LogInformation("Order {ExternalOrderId} processed successfully", erpOrderDto.ExternalOrderId);
            }
            catch (OrderValidationException ex)
            {
                logger.LogError(ex, "Order validation failed: {Message}", ex.Message);
                // For validation errors, we don't retry as they won't be fixed by retrying
                // Move to poison queue for manual review
                await MoveToPoisonQueueAsync(message, ex, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error processing order. Message will be retried or moved to poison queue.");
                // At max retries, this triggers and we move to poison queue
                await MoveToPoisonQueueAsync(message, ex, logger);
                throw;
            }
        }

        /// <summary>
        /// Processes an ERP order by creating or updating the order and customer in the database.
        /// </summary>
        private async Task ProcessOrderAsync(ERPOrderDTO erpOrderDto, ILogger logger, CancellationToken cancellationToken)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var orderRepository = new OrderRepository(dbContext);
            var customerRepository = new CustomerRepository(dbContext);
            var orderLineRepository = new OrderLineRepository(dbContext);

            // Validate the ERP order
            ValidateOrderData(erpOrderDto);

            // Check if order already exists (idempotency)
            var existingOrder = await orderRepository.GetByOrderIdAsync(erpOrderDto.ExternalOrderId!, cancellationToken);
            if (existingOrder != null)
            {
                logger.LogInformation("Order {OrderId} already exists in database. Skipping duplicate insertion.",
                    erpOrderDto.ExternalOrderId);
                return;
            }

            // Get or create customer
            var customer = await customerRepository.GetByCustomerIdAsync(erpOrderDto.ExternalCustomerId!, cancellationToken);
            if (customer == null)
            {
                customer = _mapper.Map<Customer>(erpOrderDto);
                await customerRepository.AddAsync(customer, cancellationToken);
                await customerRepository.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Created new customer {CustomerId}", erpOrderDto.ExternalCustomerId);
            }
            else
            {
                logger.LogInformation("Found existing customer {CustomerId}", erpOrderDto.ExternalCustomerId);
            }

            // Map ERP order to internal Order entity
            var order = _mapper.Map<Order>(erpOrderDto);
            order.CustomerDatabaseId = customer.Id;
            order.Status = OrderStatus.Processing;

            // Map order lines
            if (erpOrderDto.LineItems != null && erpOrderDto.LineItems.Count > 0)
            {
                order.OrderLines = erpOrderDto.LineItems
                    .Select(li => _mapper.Map<OrderLine>(li))
                    .ToList();
            }

            // Store order in database
            await orderRepository.AddAsync(order, cancellationToken);

            // Store order lines if there are any
            if (order.OrderLines.Count > 0)
            {
                foreach (var line in order.OrderLines)
                {
                    line.OrderId = order.Id;
                }
                await orderLineRepository.AddRangeAsync(order.OrderLines, cancellationToken);
            }

            // Save all changes to database
            await orderRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Order {OrderId} with {LineCount} lines successfully persisted to database",
                order.OrderId,
                order.OrderLines.Count);
        }

        /// <summary>
        /// Validates the ERP order data for required fields and business rules.
        /// </summary>
        private void ValidateOrderData(ERPOrderDTO erpOrder)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(erpOrder.ExternalOrderId))
                errors.Add("ExternalOrderId is required");

            if (string.IsNullOrWhiteSpace(erpOrder.ExternalCustomerId))
                errors.Add("ExternalCustomerId is required");

            if (erpOrder.Total <= 0)
                errors.Add("Total must be greater than zero");

            if (erpOrder.OrderDate == default)
                errors.Add("OrderDate is required");

            if (erpOrder.LineItems is null or { Count: 0 })
                errors.Add("LineItems collection is required and must contain at least one item");

            if (errors.Count > 0)
                throw new OrderValidationException($"Order validation failed: {string.Join("; ", errors)}");
        }

        /// <summary>
        /// Moves a message to the poison queue for manual review.
        /// </summary>
        private async Task MoveToPoisonQueueAsync(string message, Exception exception, ILogger logger)
        {
            try
            {
                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                if (string.IsNullOrEmpty(connectionString))
                {
                    logger.LogError("AzureWebJobsStorage configuration is missing. Cannot move message to poison queue.");
                    return;
                }

                await _poisonHandler.HandlePoisonMessageAsync(
                    message,
                    exception,
                    "orders-incoming",
                    connectionString,
                    retryCount: 3);
            }
            catch (Exception handlerException)
            {
                logger.LogError(handlerException, "Failed to move message to poison queue");
            }
        }
    }
}
