using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace OrderFunctionApp.Functions
{
    /// <summary>
    /// Function1 is a simple HTTP-triggered Azure Function template (backup/reference function).
    /// This can be used for testing and is based on the .NET 8 Isolated Worker model.
    /// </summary>
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// HTTP-triggered function that responds to GET and POST requests.
        /// </summary>
        /// <param name="req">The HTTP request</param>
        /// <returns>HTTP response with greeting message</returns>
        [Function("Function1")]
        public HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequestData req)
        {
            _logger.LogInformation("Function1 was triggered at {Timestamp}", DateTime.UtcNow);

            var response = req.CreateResponse(HttpStatusCode.OK);

            var responseData = new
            {
                message = "Hello from Azure Function 1!",
                timestamp = DateTime.UtcNow,
                environment = "Development"
            };

            response.WriteAsJsonAsync(responseData);
            return response;
        }
    }
}
