# Order Integration POC - Testing Guide

## Quick Start Testing (5 minutes)

### Prerequisites Check
```powershell
# Verify .NET 8 SDK is installed
dotnet --version
# Output should show: 8.0.x

# Verify Azure Functions Core Tools
func --version
# Output should show: 4.x.x
```

### Step 1: Start the Function App

Open **PowerShell** or **Terminal** and run:

```powershell
# Navigate to project directory
cd "C:\Users\ttkiv\source\repos\OrderIntegrationPOC\OrderFunctionApp"

# Start the local Azure Functions runtime
func start
```

**Expected output:**
```
Worker process started and initialized.
Now listening on: http://localhost:7071
Application started. Press Ctrl+C to shut down.

Functions:
	ProcessOrder: [POST] http://localhost:7071/api/orders/process
```

**Keep this terminal open** for the duration of testing.

---

## Test 1: Valid Order (Success Case)

Open a **new PowerShell terminal** and run:

```powershell
# Define the order request
$body = @{
	orderId = "ORD-20241513-001"
	customerId = "CUST-987"
	total = 149.90
	description = "Sample Electronics Order"
	orderDate = "2024-01-15T10:30:00Z"
} | ConvertTo-Json

# Send the request
$response = Invoke-RestMethod `
	-Uri "http://localhost:7071/api/orders/process" `
	-Method Post `
	-ContentType "application/json" `
	-Body $body `
	-ErrorAction Stop

# Display the response
Write-Host "`n✓ Response Status: Success (HTTP 200)" -ForegroundColor Green
$response | ConvertTo-Json | Write-Host
```

### Expected Output:
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

### Function App Console Output (func start terminal):
```
[2024-01-15T10:30:45.123Z] ProcessOrder function triggered. Request received at 1/15/2024 10:30:45 AM
[2024-01-15T10:30:45.456Z] Request body: {"orderId":"ORD-20241513-001","customerId":"CUST-987","total":149.90,...}
[2024-01-15T10:30:45.789Z] Order ORD-20241513-001 passed validation. Processing for customer CUST-987 with total 149.90.
[2024-01-15T10:30:46.012Z] Order ORD-20241513-001 processed successfully. Response sent.
```

---

## Test 2: Missing Required Field (CustomerId)

Run in your test PowerShell terminal:

```powershell
# Order WITHOUT customerId (should fail)
$body = @{
	orderId = "ORD-20241513-002"
	total = 100.00
} | ConvertTo-Json

try {
	$response = Invoke-RestMethod `
		-Uri "http://localhost:7071/api/orders/process" `
		-Method Post `
		-ContentType "application/json" `
		-Body $body `
		-ErrorAction Stop
} catch {
	# Capture the error response
	$statusCode = $_.Exception.Response.StatusCode.Value__
	$errorResponse = $_.Exception.Response.Content.ReadAsStringAsync().Result

	Write-Host "`n✗ Response Status: Error (HTTP $statusCode)" -ForegroundColor Red
	$errorResponse | Write-Host
}
```

### Expected Output:
```json
{
  "status": "error",
  "message": "Field 'customerId' is required and cannot be empty.",
  "timestamp": "2024-01-15T10:31:00.5678901Z",
  "environment": "Local Development"
}
```

**HTTP Status Code**: 400 Bad Request

### Function App Console Output:
```
[2024-01-15T10:31:00.123Z] ProcessOrder function triggered. Request received at 1/15/2024 10:31:00 AM
[2024-01-15T10:31:00.456Z] Request body: {"orderId":"ORD-20241513-002","total":100.00}
[2024-01-15T10:31:00.789Z] Validation failed: CustomerId is missing or empty.
```

---

## Test 3: Invalid Total (Zero or Negative)

Run in your test PowerShell terminal:

```powershell
# Order with zero total (should fail)
$body = @{
	orderId = "ORD-20241513-003"
	customerId = "CUST-654"
	total = 0
} | ConvertTo-Json

try {
	$response = Invoke-RestMethod `
		-Uri "http://localhost:7071/api/orders/process" `
		-Method Post `
		-ContentType "application/json" `
		-Body $body `
		-ErrorAction Stop
} catch {
	$statusCode = $_.Exception.Response.StatusCode.Value__
	$errorResponse = $_.Exception.Response.Content.ReadAsStringAsync().Result

	Write-Host "`n✗ Response Status: Error (HTTP $statusCode)" -ForegroundColor Red
	$errorResponse | Write-Host
}
```

### Expected Output:
```json
{
  "status": "error",
  "message": "Field 'total' must be greater than zero.",
  "timestamp": "2024-01-15T10:32:00.1234567Z",
  "environment": "Local Development"
}
```

---

## Test 4: Invalid JSON Format

Run in your test PowerShell terminal:

```powershell
# Malformed JSON (should fail)
$body = '{orderId: "ORD-20241513-004", customerId: "CUST-789"}'  # Missing quotes around keys

try {
	$response = Invoke-RestMethod `
		-Uri "http://localhost:7071/api/orders/process" `
		-Method Post `
		-ContentType "application/json" `
		-Body $body `
		-ErrorAction Stop
} catch {
	$statusCode = $_.Exception.Response.StatusCode.Value__
	$errorResponse = $_.Exception.Response.Content.ReadAsStringAsync().Result

	Write-Host "`n✗ Response Status: Error (HTTP $statusCode)" -ForegroundColor Red
	$errorResponse | Write-Host
}
```

### Expected Output:
```json
{
  "status": "error",
  "message": "Invalid JSON format. Please check your request body.",
  "timestamp": "2024-01-15T10:33:00.9876543Z",
  "environment": "Local Development"
}
```

---

## Test 5: Batch Testing Multiple Orders

Create a file `batch-test.ps1` in your project directory:

```powershell
# batch-test.ps1
# Test multiple orders in sequence

$testCases = @(
	@{
		name = "Valid Order 1"
		order = @{
			orderId = "ORD-2024-BATCH-001"
			customerId = "CUST-100"
			total = 199.99
			description = "Laptop"
		}
		expectSuccess = $true
	},
	@{
		name = "Valid Order 2"
		order = @{
			orderId = "ORD-2024-BATCH-002"
			customerId = "CUST-101"
			total = 49.99
			description = "Mouse"
		}
		expectSuccess = $true
	},
	@{
		name = "Invalid Order - Missing OrderId"
		order = @{
			customerId = "CUST-102"
			total = 100.00
		}
		expectSuccess = $false
	},
	@{
		name = "Invalid Order - Negative Total"
		order = @{
			orderId = "ORD-2024-BATCH-003"
			customerId = "CUST-103"
			total = -50.00
		}
		expectSuccess = $false
	}
)

$passCount = 0
$failCount = 0

foreach ($testCase in $testCases) {
	Write-Host "`nTesting: $($testCase.name)" -ForegroundColor Cyan
	Write-Host "Order: $($testCase.order | ConvertTo-Json -Compress)"

	$body = $testCase.order | ConvertTo-Json

	try {
		$response = Invoke-RestMethod `
			-Uri "http://localhost:7071/api/orders/process" `
			-Method Post `
			-ContentType "application/json" `
			-Body $body `
			-ErrorAction Stop

		if ($testCase.expectSuccess) {
			Write-Host "✓ PASS: Got expected success response" -ForegroundColor Green
			$passCount++
		} else {
			Write-Host "✗ FAIL: Expected error but got success" -ForegroundColor Red
			$failCount++
		}

		Write-Host "Response: $($response.message)"
	} catch {
		if (-not $testCase.expectSuccess) {
			Write-Host "✓ PASS: Got expected error response" -ForegroundColor Green
			$passCount++
		} else {
			Write-Host "✗ FAIL: Expected success but got error" -ForegroundColor Red
			$failCount++
		}

		$errorResponse = $_.Exception.Response.Content.ReadAsStringAsync().Result | ConvertFrom-Json
		Write-Host "Error: $($errorResponse.message)"
	}
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test Results: $passCount Passed, $failCount Failed" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
```

Run the batch test:
```powershell
cd "C:\Users\ttkiv\source\repos\OrderIntegrationPOC"
.\batch-test.ps1
```

---

## Test 6: Performance Testing (Load Test)

```powershell
# Simple load test - send 10 requests in sequence
Write-Host "Starting Performance Test..." -ForegroundColor Yellow

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$requestCount = 10

for ($i = 1; $i -le $requestCount; $i++) {
	$body = @{
		orderId = "PERF-TEST-$i"
		customerId = "CUST-PERF-$i"
		total = 100.00
	} | ConvertTo-Json

	try {
		$response = Invoke-RestMethod `
			-Uri "http://localhost:7071/api/orders/process" `
			-Method Post `
			-ContentType "application/json" `
			-Body $body `
			-TimeoutSec 30 `
			-ErrorAction Stop

		Write-Host "Request $i : ✓ Success (Status: $($response.status))"
	} catch {
		Write-Host "Request $i : ✗ Error"
	}
}

$stopwatch.Stop()
$averageTime = $stopwatch.ElapsedMilliseconds / $requestCount

Write-Host "`nPerformance Summary:" -ForegroundColor Green
Write-Host "Total Requests: $requestCount"
Write-Host "Total Time: $($stopwatch.ElapsedMilliseconds) ms"
Write-Host "Average Time per Request: $averageTime ms"
Write-Host "Requests per Second: $(1000 / $averageTime)"
```

---

## Test 7: Using Visual Studio REST Client Extension

**Install REST Client extension** in Visual Studio Code:
1. Open VS Code
2. Go to Extensions (Ctrl+Shift+X)
3. Search for "REST Client" by Huachao Mao
4. Click Install

Create a file named `test.http` in your project:

```http
### Test 1: Valid Order
POST http://localhost:7071/api/orders/process HTTP/1.1
Content-Type: application/json

{
  "orderId": "ORD-20241513-REST-001",
  "customerId": "CUST-REST-001",
  "total": 299.99,
  "description": "REST Client Test"
}

### Test 2: Missing CustomerId
POST http://localhost:7071/api/orders/process HTTP/1.1
Content-Type: application/json

{
  "orderId": "ORD-20241513-REST-002",
  "total": 150.00
}

### Test 3: Negative Total
POST http://localhost:7071/api/orders/process HTTP/1.1
Content-Type: application/json

{
  "orderId": "ORD-20241513-REST-003",
  "customerId": "CUST-REST-003",
  "total": -99.99
}

### Test 4: High-Value Order
POST http://localhost:7071/api/orders/process HTTP/1.1
Content-Type: application/json

{
  "orderId": "ORD-20241513-REST-004",
  "customerId": "CUST-ENTERPRISE",
  "total": 9999.99,
  "description": "Enterprise License"
}
```

**To run tests:**
- Click "Send Request" above each request in VS Code
- Results appear in the side panel

---

## Debugging in Visual Studio

### Enable Breakpoint Debugging

1. Open Visual Studio with the solution
2. Right-click `OrderFunctionApp` → Set as Startup Project
3. Open `OrderFunctionApp/Functions/ProcessOrder.cs`
4. Click left margin to set breakpoint at line 29 (Run method entry)
5. Press **F5** to start debugging
6. Run a test request using PowerShell
7. Execution will pause at the breakpoint
8. Use Visual Studio debugging tools:
   - **F10**: Step over
   - **F11**: Step into
   - **Shift+F11**: Step out
   - **Ctrl+Alt+W**: Watch window (add variables to monitor)
   - **Hover** over variables to see current value

---

## Integration Testing with Service Bus (Optional - Requires Azure)

If you have an Azure subscription and Service Bus provisioned:

```powershell
# Prerequisites
# 1. Create Service Bus Namespace in Azure Portal
# 2. Create Queue: orders-incoming
# 3. Get connection string from Shared Access Policies

# Set environment variables
$env:ServiceBusConnection = "Endpoint=sb://YOUR-NAMESPACE.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR-KEY"

# Install Service Bus tools (optional)
dotnet tool install Azure.Messaging.ServiceBus

# Send message to Service Bus (PowerShell script)
$connectionString = $env:ServiceBusConnection
$queueName = "orders-incoming"

$order = @{
	orderId = "ORD-2024-SB-001"
	customerId = "CUST-SB-001"
	total = 500.00
} | ConvertTo-Json

# Use Azure CLI to send message
az servicebus queue create `
	--resource-group "YourResourceGroup" `
	--namespace-name "your-namespace" `
	--name "orders-incoming"

# Send test message
# (Use Azure Portal or Service Bus Explorer tool)
```

---

## Monitoring and Logs

### View Function Logs

**Option 1: Visual Studio Output Window**
- Start debugging (F5)
- Look at the Output window (View > Output)
- Select "Azure Functions" from dropdown to filter logs

**Option 2: Terminal Output**
- When running `func start`, all logs appear in the terminal:
```
[2024-01-15T10:30:45.123Z] ProcessOrder function triggered...
[2024-01-15T10:30:46.456Z] Request body: {...}
...
```

**Option 3: Application Insights (If Configured)**
- Azure Portal > Application Insights > Logs (KQL queries)
- Run query:
```kusto
traces
| where message contains "ProcessOrder"
| project timestamp, message, severity = severityLevel
| order by timestamp desc
```

---

## Cleanup After Testing

```powershell
# Stop the Azure Functions runtime
# In the func start terminal: Press Ctrl+C

# Optional: Clear local cache
Remove-Item -Recurse -Force "$env:USERPROFILE\.azure\functions"

# Optional: Stop Azurite if running
# In Azurite terminal: Press Ctrl+C
```

---

## Troubleshooting During Testing

### Test fails with "Connection refused"
- Ensure Function App is running (`func start`)
- Check port 7071 is not blocked by firewall
- Try running from same machine where func is running

### Test returns "Invalid JSON format"
- Verify JSON syntax using JSONLint.com
- Ensure quotes are correct: `"key": "value"`
- In PowerShell, use `| ConvertTo-Json` to ensure proper formatting

### Slow response times
- Check if machine resources are low (RAM, CPU, disk)
- Try simpler test without complex logic
- Monitor from System Task Manager while running load test

### Function crashes during debugging
- Check .NET version: `dotnet --version`
- Verify all NuGet packages are restored
- Rebuild solution: Ctrl+Shift+B

---

## Test Completion Checklist

- [ ] Test 1 (Valid Order): ✓ Passed
- [ ] Test 2 (Missing CustomerId): ✓ Passed
- [ ] Test 3 (Invalid Total): ✓ Passed
- [ ] Test 4 (Invalid JSON): ✓ Passed
- [ ] Test 5 (Batch Testing): ✓ Passed
- [ ] Test 6 (Performance): ✓ Completed
- [ ] Test 7 (Visual Studio): ✓ Passed
- [ ] All logs appear in console: ✓ Verified
- [ ] Error handling works correctly: ✓ Confirmed
- [ ] Response formats are correct: ✓ Validated

---

## Next Steps

- Deploy to Azure using `func azure functionapp publish`
- Set up Logic App in Azure Portal
- Configure Application Insights monitoring
- Implement database integration
- Add more comprehensive unit tests

---

**Happy Testing!** 🚀

For issues, refer to the main [README.md](../README.md) or Azure documentation.
