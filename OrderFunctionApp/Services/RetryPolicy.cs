using Microsoft.Extensions.Logging;

namespace OrderFunctionApp.Services
{
    /// <summary>
    /// Provides retry policies and utilities for handling transient failures.
    /// Implements exponential backoff strategies for reliable processing.
    /// </summary>
    public class RetryPolicy
    {
        private readonly ILogger<RetryPolicy> _logger;

        public RetryPolicy(ILogger<RetryPolicy> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes an async operation with exponential backoff retry policy.
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="operation">The async operation to execute.</param>
        /// <param name="operationName">Name of the operation for logging purposes.</param>
        /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
        /// <param name="initialDelayMs">Initial delay in milliseconds (default: 1000).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="AggregateException">Thrown when all retries are exhausted.</exception>
        public async Task<T> ExecuteWithRetryAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            string operationName,
            int maxRetries = 3,
            int initialDelayMs = 1000,
            CancellationToken cancellationToken = default)
        {
            var lastException = (Exception?)null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation(
                        "Executing operation '{OperationName}' - Attempt {Attempt} of {MaxRetries}",
                        operationName, attempt, maxRetries);

                    return await operation(cancellationToken);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning(
                        ex,
                        "Operation '{OperationName}' failed on attempt {Attempt} of {MaxRetries}. " +
                        "Error: {Error}",
                        operationName, attempt, maxRetries, ex.Message);

                    if (attempt < maxRetries)
                    {
                        var delay = CalculateDelay(attempt, initialDelayMs);
                        _logger.LogInformation(
                            "Retrying operation '{OperationName}' after {DelayMs}ms delay",
                            operationName, delay);

                        await Task.Delay(delay, cancellationToken);
                    }
                }
            }

            _logger.LogError(
                lastException,
                "Operation '{OperationName}' failed after {MaxRetries} attempts",
                operationName, maxRetries);

            throw new InvalidOperationException(
                $"Operation '{operationName}' failed after {maxRetries} attempts",
                lastException);
        }

        /// <summary>
        /// Executes an async operation with exponential backoff retry policy.
        /// </summary>
        /// <param name="operation">The async operation to execute.</param>
        /// <param name="operationName">Name of the operation for logging purposes.</param>
        /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
        /// <param name="initialDelayMs">Initial delay in milliseconds (default: 1000).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteWithRetryAsync(
            Func<CancellationToken, Task> operation,
            string operationName,
            int maxRetries = 3,
            int initialDelayMs = 1000,
            CancellationToken cancellationToken = default)
        {
            var lastException = (Exception?)null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation(
                        "Executing operation '{OperationName}' - Attempt {Attempt} of {MaxRetries}",
                        operationName, attempt, maxRetries);

                    await operation(cancellationToken);
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning(
                        ex,
                        "Operation '{OperationName}' failed on attempt {Attempt} of {MaxRetries}. " +
                        "Error: {Error}",
                        operationName, attempt, maxRetries, ex.Message);

                    if (attempt < maxRetries)
                    {
                        var delay = CalculateDelay(attempt, initialDelayMs);
                        _logger.LogInformation(
                            "Retrying operation '{OperationName}' after {DelayMs}ms delay",
                            operationName, delay);

                        await Task.Delay(delay, cancellationToken);
                    }
                }
            }

            _logger.LogError(
                lastException,
                "Operation '{OperationName}' failed after {MaxRetries} attempts",
                operationName, maxRetries);

            throw new InvalidOperationException(
                $"Operation '{operationName}' failed after {maxRetries} attempts",
                lastException);
        }

        /// <summary>
        /// Calculates the delay for exponential backoff.
        /// Formula: initialDelay * (2 ^ (attempt - 1)) with a maximum of 32 seconds.
        /// </summary>
        /// <param name="attempt">The current attempt number (1-based).</param>
        /// <param name="initialDelayMs">The initial delay in milliseconds.</param>
        /// <returns>The delay in milliseconds.</returns>
        private int CalculateDelay(int attempt, int initialDelayMs)
        {
            const int maxDelayMs = 32000; // 32 seconds max
            var exponentialDelay = initialDelayMs * (int)Math.Pow(2, attempt - 1);
            return Math.Min(exponentialDelay, maxDelayMs);
        }
    }
}
