# ProcessOrder API Reference

## Endpoint Overview

The ProcessOrder API is an Azure Function that processes customer orders via HTTP POST requests.

## API Details

### Base URL (Local Development)
```
http://localhost:7071/api/orders/process
```

### Base URL (Azure Production)
```
https://{functionAppName}.azurewebsites.net/api/orders/process?code={functionCode}
```

---

## HTTP Method

```
POST
```

---

## Request

### Headers

| Header | Value | Required |
|--------|-------|----------|
| `Content-Type` | `application/json` | Yes |

### Body

JSON object containing order details:

```json
{
  "orderId": "string",
  "customerId": "string",
  "total": number,
  "description": "string (optional)",
  "orderDate": "ISO 8601 datetime (optional)"
}
```

### Field Definitions

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `orderId` | string | Yes | Non-empty | Unique order identifier |
| `customerId` | string | Yes | Non-empty | Unique customer identifier |
| `total` | decimal | No | > 0 | Order total amount |
| `description` | string | No | None | Order description or notes |
| `orderDate` | string | No | ISO 8601 format | Order creation date/time |

### Example Request

**cURL:**
```bash
curl -X POST http://localhost:7071/api/orders/process \
  -H "Content-Type: application/json" \
  -d '{
	"orderId": "ORD-20241513-001",
	"customerId": "CUST-987",
	"total": 149.90,
	"description": "Sample Electronics Order",
	"orderDate": "2024-01-15T10:30:00Z"
  }'
```

**PowerShell:**
```powershell
$body = @{
	orderId = "ORD-20241513-001"
	customerId = "CUST-987"
	total = 149.90
	description = "Sample Electronics Order"
	orderDate = "2024-01-15T10:30:00Z"
} | ConvertTo-Json

Invoke-RestMethod `
	-Uri "http://localhost:7071/api/orders/process" `
	-Method Post `
	-ContentType "application/json" `
	-Body $body
```

**JavaScript (Fetch API):**
```javascript
const order = {
	orderId: "ORD-20241513-001",
	customerId: "CUST-987",
	total: 149.90,
	description: "Sample Electronics Order",
	orderDate: "2024-01-15T10:30:00Z"
};

fetch('http://localhost:7071/api/orders/process', {
	method: 'POST',
	headers: {
		'Content-Type': 'application/json'
	},
	body: JSON.stringify(order)
})
.then(response => response.json())
.then(data => console.log('Success:', data))
.catch(error => console.error('Error:', error));
```

---

## Response

### Success Response (HTTP 200 OK)

```json
{
  "status": "success",
  "message": "Order processed successfully",
  "orderId": "ORD-20241513-001",
  "customerId": "CUST-987",
  "total": 149.90,
  "processedAt": "2024-01-15T10:30:45.1234567Z",
  "environment": "Local Development"
}
```

### Field Definitions (Success Response)

| Field | Type | Description |
|-------|------|-------------|
| `status` | string | Always "success" for 200 responses |
| `message` | string | Confirmation message |
| `orderId` | string | The processed order ID |
| `customerId` | string | The customer ID from the order |
| `total` | decimal | The order total amount |
| `processedAt` | string | ISO 8601 timestamp when order was processed |
| `environment` | string | Deployment environment |

---

## Error Responses

### HTTP 400 Bad Request

**Scenario 1: Missing orderId**
```json
{
  "status": "error",
  "message": "Field 'orderId' is required and cannot be empty.",
  "timestamp": "2024-01-15T10:31:00.5678901Z",
  "environment": "Local Development"
}
```

**Scenario 2: Missing customerId**
```json
{
  "status": "error",
  "message": "Field 'customerId' is required and cannot be empty.",
  "timestamp": "2024-01-15T10:31:15.9876543Z",
  "environment": "Local Development"
}
```

**Scenario 3: Invalid total (zero or negative)**
```json
{
  "status": "error",
  "message": "Field 'total' must be greater than zero.",
  "timestamp": "2024-01-15T10:32:00.1234567Z",
  "environment": "Local Development"
}
```

**Scenario 4: Invalid JSON format**
```json
{
  "status": "error",
  "message": "Invalid JSON format. Please check your request body.",
  "timestamp": "2024-01-15T10:33:00.9876543Z",
  "environment": "Local Development"
}
```

### HTTP 500 Internal Server Error

```json
{
  "status": "error",
  "message": "An unexpected error occurred while processing the order. Please contact support.",
  "timestamp": "2024-01-15T10:34:00.1111111Z",
  "environment": "Local Development"
}
```

---

## Error Codes and Handling

| HTTP Status | Error Message | Action |
|------------|-------------|--------|
| 400 | Missing 'orderId' | Provide required field |
| 400 | Missing 'customerId' | Provide required field |
| 400 | Invalid 'total' | Ensure total > 0 |
| 400 | Invalid JSON format | Fix JSON syntax |
| 500 | Unexpected error | Retry or contact support |

---

## Validation Rules

### orderId
- **Type**: String
- **Required**: Yes
- **Max Length**: Unlimited (recommended: ≤50 characters)
- **Validation**: Non-empty
- **Examples**: `ORD-20241513-001`, `ORDER123456`, `PO-2024-001`

### customerId
- **Type**: String
- **Required**: Yes
- **Max Length**: Unlimited (recommended: ≤50 characters)
- **Validation**: Non-empty
- **Examples**: `CUST-987`, `CUSTOMER001`, `C12345`

### total
- **Type**: Decimal (number)
- **Required**: No (but validated if provided)
- **Min Value**: > 0
- **Decimal Places**: Up to 2 (currency)
- **Examples**: `149.90`, `9999.99`, `0.01`

### description
- **Type**: String
- **Required**: No
- **Max Length**: Unlimited
- **Examples**: `"Sample Electronics Order"`, `"Enterprise License"`

### orderDate
- **Type**: String (ISO 8601)
- **Required**: No
- **Format**: `YYYY-MM-DDTHH:mm:ssZ`
- **Examples**: `"2024-01-15T10:30:00Z"`, `"2024-01-15T14:45:30.123Z"`

---

## Rate Limiting

- **No explicit rate limiting** on the Function App by default
- **Azure Functions** auto-scales based on demand
- **Service Bus** has message throughput limits (see Azure documentation)

---

## Authentication

### Local Development (HTTP Trigger)
- **No authentication** required for local testing
- Authorization Level: `Function` (uses function code)

### Azure Production
- **Function Code**: Required in query string
  ```
  ?code=YOUR_FUNCTION_CODE
  ```
- **Alternative**: Azure AD or other identity provider (configurable)

---

## Logging and Monitoring

All requests are logged to:
1. **Azure Functions Console** (runtime logs)
2. **Application Insights** (telemetry and custom events)

### Available Logs

- Function trigger event
- Request body received
- Validation steps
- Processing status
- Response sent

### Example Application Insights KQL Query

```kusto
traces
| where message contains "ProcessOrder"
| where operation_Name == "ProcessOrder"
| project timestamp, message, severityLevel, customDimensions
| order by timestamp desc
| limit 100
```

---

## Best Practices

### Request Handling
1. Always include `Content-Type: application/json` header
2. Ensure JSON is well-formed before sending
3. Use unique, meaningful orderId values
4. Include description for tracking

### Error Handling
1. Always check HTTP status code first
2. Parse error response for specific error message
3. Implement retry logic for 500 errors (exponential backoff)
4. Log all errors for troubleshooting

### Performance
1. Batch orders if processing multiple (send individually though)
2. Use connection pooling for multiple requests
3. Monitor response times in Application Insights
4. Implement timeout (30 seconds recommended)

---

## Integration Examples

### Azure Logic App Integration

Logic App calls this endpoint when Service Bus message arrives:

```json
{
  "method": "post",
  "uri": "@parameters('FunctionProcessOrderUri')",
  "headers": {
	"Content-Type": "application/json"
  },
  "body": "@body('Parse_JSON')"
}
```

### Service Bus to Function Flow

```
Service Bus Message (JSON)
		 ↓
Logic App Trigger (Polling)
		 ↓
Parse JSON
		 ↓
HTTP Call to ProcessOrder
		 ↓
Function Validates & Processes
		 ↓
Log to Application Insights
		 ↓
Response to Logic App
		 ↓
Success/Error Action
```

---

## Testing Tools

### Visual Studio Code REST Client Extension
Create `test.http`:
```http
### Test Valid Order
POST http://localhost:7071/api/orders/process HTTP/1.1
Content-Type: application/json

{
  "orderId": "ORD-TEST-001",
  "customerId": "TEST-001",
  "total": 99.99
}
```

### Postman
- Import collection for automated testing
- Set variables for baseUrl and function code
- Create test scripts for response validation

### Azure Portal Function Testing
- Use **Test/Run** tab in Azure Portal
- Pass JSON in request body
- View response and logs

---

## Change Log

### Version 1.0 (Current)
- Initial API release
- Order validation
- Application Insights integration
- Error handling

---

## Support

For issues or questions:
1. Check the [Testing Guide](Testing-Guide.md)
2. Review [README.md](../README.md)
3. Check Application Insights logs in Azure Portal
4. Consult [Deployment Guide](Deployment-Guide.md)

---

**API Documentation Version**: 1.0  
**Last Updated**: January 15, 2024
