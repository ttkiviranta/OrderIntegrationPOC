using System.Net;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace OrderFunctionApp.Functions
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Function1 was triggered at {Timestamp}", DateTime.UtcNow);

            var response = req.CreateResponse(HttpStatusCode.OK);

            var responseData = new
            {
                message = "Hello from Azure Function 1!",
                timestamp = DateTime.UtcNow.ToString("O"),
                environment = "Development"
            };

            try
            {
                // Write directly to body to avoid FormatException
                var json = JsonSerializer.Serialize(responseData);
                var bytes = Encoding.UTF8.GetBytes(json);
                response.Body.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing response");
            }

            return response;
        }
    }
}
