using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace OrderFunctionApp.Services
{
    /// <summary>
    /// Handles poison messages that cannot be processed after maximum retries.
    /// Poison messages are moved to a dead-letter queue for manual inspection and recovery.
    /// </summary>
    public class PoisonMessageHandler
    {
        private readonly ILogger<PoisonMessageHandler> _logger;

        public PoisonMessageHandler(ILogger<PoisonMessageHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles a poison message by moving it to the dead-letter queue.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="exception">The exception that occurred while processing the message.</param>
        /// <param name="queueName">The name of the original queue.</param>
        /// <param name="connectionString">The storage account connection string.</param>
        /// <param name="retryCount">The number of times the message was retried.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandlePoisonMessageAsync(
            string message,
            Exception exception,
            string queueName,
            string connectionString,
            int retryCount = 3,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogError(
                    exception,
                    "Poison message detected for queue '{QueueName}'. " +
                    "Message will be moved to dead-letter queue after {RetryCount} retries.",
                    queueName, retryCount);

                const string dlqSuffix = "-dlq";
                var dlqName = $"{queueName}{dlqSuffix}";

                var dlqClient = new QueueClient(
                    connectionString,
                    dlqName,
                    new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });

                // Ensure DLQ exists
                await dlqClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

                // Create metadata about the poison message
                var poisonMetadata = new
                {
                    originalQueue = queueName,
                    originalMessage = message,
                    exceptionMessage = exception.Message,
                    exceptionStackTrace = exception.StackTrace,
                    retryCount = retryCount,
                    timestamp = DateTime.UtcNow
                };

                var dlqMessage = JsonSerializer.Serialize(poisonMetadata, new JsonSerializerOptions { WriteIndented = true });

                // Send to DLQ
                await dlqClient.SendMessageAsync(dlqMessage, cancellationToken: cancellationToken);

                _logger.LogInformation(
                    "Poison message successfully moved to '{DeadLetterQueue}'. " +
                    "Manual review and recovery may be required.",
                    dlqName);
            }
            catch (Exception handlerException)
            {
                _logger.LogError(
                    handlerException,
                    "Failed to handle poison message. This message will require manual intervention. " +
                    "Original error: {OriginalError}",
                    exception.Message);
            }
        }
    }
}
