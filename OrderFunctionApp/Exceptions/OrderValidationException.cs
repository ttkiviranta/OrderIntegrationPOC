namespace OrderFunctionApp.Exceptions
{
    /// <summary>
    /// Exception thrown when an order validation fails.
    /// This is used for business rule violations during order processing.
    /// </summary>
    public class OrderValidationException : Exception
    {
        public OrderValidationException(string message) : base(message)
        {
        }

        public OrderValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
