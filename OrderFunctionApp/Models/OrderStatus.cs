namespace OrderFunctionApp.Models
{
    /// <summary>
    /// Defines the status of an order in the system.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Order has been received and awaiting processing.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Order is currently being processed.
        /// </summary>
        Processing = 1,

        /// <summary>
        /// Order has been successfully completed.
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Order has been cancelled.
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// Order failed to process.
        /// </summary>
        Failed = 4
    }
}
