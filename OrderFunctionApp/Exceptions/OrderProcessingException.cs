namespace OrderFunctionApp.Exceptions
{
    /// <summary>
    /// Exception thrown when an order processing operation fails.
    /// This is used for transient or permanent failures during order handling.
    /// </summary>
    public class OrderProcessingException : Exception
    {
        /// <summary>
        /// Gets a value indicating whether the error is transient (can be retried).
        /// </summary>
        public bool IsTransient { get; }

        public OrderProcessingException(string message, bool isTransient = true)
            : base(message)
        {
            IsTransient = isTransient;
        }

        public OrderProcessingException(string message, Exception innerException, bool isTransient = true)
            : base(message, innerException)
        {
            IsTransient = isTransient;
        }
    }
}
