# Order Integration Proof of Concept (POC)

## Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Solution Structure](#solution-structure)
- [Setup and Configuration](#setup-and-configuration)
- [Running the POC](#running-the-poc)
- [Testing the Solution](#testing-the-solution)
- [Troubleshooting](#troubleshooting)
- [Future Enhancements](#future-enhancements)

---

## Overview

This Proof of Concept demonstrates an end-to-end order processing pipeline integrating:
- **Azure Service Bus** for asynchronous message queuing
- **Azure Logic App** for workflow orchestration
- **Azure Functions** for serverless order processing
- **Application Insights** for monitoring and telemetry

### Business Scenario

An ERP system simulates sending customer orders to an Azure Service Bus queue. A Logic App automatically triggers when orders arrive, validates the payload, calls an Azure Function for processing, and logs all activities to Application Insights.

---

## Architecture

### Component Diagram (Text-based)

```
┌──────────────────────────────────────────────────────────────────┐
│                         ORDER PROCESSING FLOW                    │
└──────────────────────────────────────────────────────────────────┘

	[ERP System]
		 │
		 │ Sends JSON Order
		 ▼
	[Azure Service Bus Queue: orders-incoming]
		 │
		 │ Message Arrives (Polling/Trigger)
		 ▼
	[Azure Logic App: OrderProcessingWorkflow]
		 │
		 ├─► Parse JSON Message
		 │
		 ├─► Validate Order Structure (JSON Schema)
		 │
		 ├─► Call Azure Function (HTTP POST)
		 ▼
	[Azure Function: ProcessOrder]
		 │
		 ├─► Deserialize JSON Payload
		 │
		 ├─► Validate Required Fields (orderId, customerId)
		 │
		 ├─► Validate Business Rules (total > 0)
		 │
		 ├─► Log Results to Application Insights
		 │
		 └─► Return HTTP 200 (Success) or 400 (Error)
		 │
		 ▼
	[Logic App: Log to Application Insights]
		 │
		 ├─► Success Logging
		 │
		 └─► Error Handling & Retry Logic
		 │
		 ▼
	[Application Insights Dashboard]
		 │
		 ├─► Performance Metrics
		 │
		 ├─► Error Logs
		 │
		 └─► Custom Telemetry
```

### Data Flow

1. **Message Format**: JSON payload containing order details
2. **Queue Name**: `orders-incoming`
3. **Processing**: Synchronous validation with error handling
4. **Response**: HTTP 200 OK or 400 Bad Request with diagnostic information

---

## Prerequisites

Before setting up the POC, ensure you have:

### Software Requirements
- **Visual Studio 2022** or later (or Visual Studio Code with Azure Functions extension)
- **.NET 8 SDK** installed
- **Azure Functions Core Tools** (v4.x or later)
- **PowerShell** or terminal of your choice

### Azure Resources (Optional for Local Testing)
- Azure Subscription (for cloud deployment)
- Azure Storage Account (for Azure Functions runtime state)
- Azure Service Bus (Standard or Premium tier)
- Azure LogicApps resource
- Application Insights instance

### Local Development (Storage Emulator)
- **Azure Storage Emulator** or **Azurite** (for local development)

### Tools and CLIs
```powershell
# Install Azure Functions Core Tools
npm install -g azure-functions-core-tools@4 --unsafe-perm true

# Install Azure CLI (optional, for cloud deployment)
# Download from: https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows
```

---

## Solution Structure

```
OrderIntegrationPOC/
│
├── OrderFunctionApp/                  # Azure Functions Project
│   ├── Functions/
│   │   └── ProcessOrder.cs            # HTTP-triggered function for order processing
│   │
│   ├── Models/
│   │   ├── OrderRequest.cs            # Order data model
│   │   └── OrderValidationResult.cs   # Validation result model
│   │
│   ├── Services/
│   │   └── OrderValidator.cs          # Business validation logic
│   │
│   ├── Properties/
│   │   └── launchSettings.json        # Local debug settings
│   │
│   ├── Program.cs                     # Dependency injection & configuration
│   ├── host.json                      # Function runtime configuration
│   ├── local.settings.json            # Local development settings
│   └── OrderFunctionApp.csproj        # Project file
│
├── docs/                              # Documentation and Configuration
│   ├── sample-payloads.json           # Example order JSON payloads
│   ├── LogicApp-Template.json         # ARM template for infrastructure
│   ├── LogicApp-Definition.json       # Logic App workflow definition
│   └── Testing-Guide.md               # Step-by-step testing instructions
│
├── README.md                          # This file
└── OrderIntegrationPOC.slnx           # Visual Studio Solution file

```

---

## Setup and Configuration

### Step 1: Clone or Extract Solution

Extract the OrderIntegrationPOC solution to your local machine:
```powershell
cd C:\Users\[YourUsername]\source\repos
# Solution is already extracted to: C:\Users\ttkiv\source\repos\OrderIntegrationPOC
```

### Step 2: Open Solution in Visual Studio

```powershell
# Navigate to solution directory
cd C:\Users\ttkiv\source\repos\OrderIntegrationPOC

# Open in Visual Studio
start OrderIntegrationPOC.slnx
```

### Step 3: Configure Local Settings

Edit `OrderFunctionApp/local.settings.json`:

```json
{
	"IsEncrypted": false,
	"Values": {
		"AzureWebJobsStorage": "UseDevelopmentStorage=true",
		"FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
		"APPINSIGHTS_INSTRUMENTATIONKEY": "YOUR_APPINSIGHTS_INSTRUMENTATION_KEY_HERE",
		"ServiceBusConnection": "Your-Service-Bus-Connection-String",
		"ServiceBusQueueName": "orders-incoming",
		"ENVIRONMENT": "Development"
	}
}
```

**Note for local testing without Azure resources**:
- Keep `AzureWebJobsStorage` as `UseDevelopmentStorage=true`
- Application Insights key can be a placeholder for local testing
- Service Bus connection string is needed only for actual cloud testing

### Step 4: Restore NuGet Packages

In Visual Studio:
1. Right-click on the solution → **Restore NuGet Packages**
2. Or run in Package Manager Console:
```powershell
dotnet restore
```

### Step 5: Build the Solution

```powershell
# In Visual Studio: Ctrl+Shift+B
# Or in terminal:
dotnet build
```

### Optional: Enable Azure Storage Emulator

For local testing with Service Bus simulation:

```powershell
# Start Azurite (modern alternative to Storage Emulator)
npm install -g azurite
azurite --silent --location c:\azurite

# Or use Azure Storage Emulator (legacy)
# C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe start
```

---

## Running the POC

### Local Development Mode

#### Option 1: Running in Visual Studio

1. Open the solution in Visual Studio
2. Set `OrderFunctionApp` as the startup project
3. Press **F5** or click the green **Run** button
4. The Azure Functions runtime will start locally on `http://localhost:7071`

Expected output:
```
Worker process started and initialized.
Now listening on: http://localhost:7071
Application started. Press Ctrl+C to shut down.

Functions:
	ProcessOrder: [POST] http://localhost:7071/api/orders/process
```

#### Option 2: Running from Command Line

```powershell
# Navigate to the function app directory
cd C:\Users\ttkiv\source\repos\OrderIntegrationPOC\OrderFunctionApp

# Start the Azure Functions runtime
func start

# Output:
# Azure Functions Core Tools (version 4.x)
# Function Runtime Version: 4.x
# Listening on: http://localhost:7071
```

#### Option 3: Running in Visual Studio Code

```powershell
# Open the solution in VS Code
code C:\Users\ttkiv\source\repos\OrderIntegrationPOC

# Install Azure Functions extension (if not already installed)
# Then press F5 to start debugging
```

---

## Testing the Solution

### Test Scenario 1: Successful Order Processing

1. **Start the Function App** (as described above)
2. **Send a Valid Order** using one of the methods below:

#### Method A: Using cURL (Windows Terminal)

```powershell
$order = @{
	orderId = "ORD-20241513-001"
	customerId = "CUST-987"
	total = 149.90
	description = "Sample Electronics Order"
}

curl -X POST http://localhost:7071/api/orders/process `
  -H "Content-Type: application/json" `
  -d ($order | ConvertTo-Json)
```

#### Method B: Using PowerShell Invoke-RestMethod

```powershell
$body = @{
	orderId = "ORD-20241513-001"
	customerId = "CUST-987"
	total = 149.90
} | ConvertTo-Json

$response = Invoke-RestMethod `
	-Uri "http://localhost:7071/api/orders/process" `
	-Method Post `
	-ContentType "application/json" `
	-Body $body

$response | ConvertTo-Json
```

#### Method C: Using Visual Studio REST Client (Code extension)

Create a file `test.http`:
```http
### Test 1: Valid Order
POST http://localhost:7071/api/orders/process HTTP/1.1
Content-Type: application/json

{
  "orderId": "ORD-20241513-001",
  "customerId": "CUST-987",
  "total": 149.90,
  "description": "Sample Electronics Order"
}

### Test 2: Missing CustomerId (Should Fail)
POST http://localhost:7071/api/orders/process HTTP/1.1
Content-Type: application/json

{
  "orderId": "ORD-20241513-002",
  "total": 100.00
}

### Test 3: Negative Total (Should Fail)
POST http://localhost:7071/api/orders/process HTTP/1.1
Content-Type: application/json

{
  "orderId": "ORD-20241513-003",
  "customerId": "CUST-654",
  "total": -50.00
}
```

### Expected Responses

#### Success Response (HTTP 200 OK)
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

#### Error Response (HTTP 400 Bad Request)
```json
{
  "status": "error",
  "message": "Field 'customerId' is required and cannot be empty.",
  "timestamp": "2024-01-15T10:31:00.5678901Z",
  "environment": "Local Development"
}
```

### Test Scenario 2: Multiple Test Cases

Use the payloads in `docs/sample-payloads.json`:

```powershell
# Load sample payloads
$payloads = Get-Content -Path "docs\sample-payloads.json" | ConvertFrom-Json

# Test each valid order
$payloads.examples | Where-Object { $_.name -like "Valid Order*" } | ForEach-Object {
	Write-Host "Testing: $($_.name)"

	$body = $_.payload | ConvertTo-Json

	try {
		$response = Invoke-RestMethod `
			-Uri "http://localhost:7071/api/orders/process" `
			-Method Post `
			-ContentType "application/json" `
			-Body $body

		Write-Host "✓ Success: $($response.status)"
	} catch {
		Write-Host "✗ Error: $_"
	}
}
```

### Monitoring Logs

#### View Function Logs in Visual Studio

In Visual Studio Output window (when debugging):
- Select **Azure Functions** from the dropdown to see function logs

#### View Console Output

When running `func start` from command line:
```
[2024-01-15T10:30:45.123Z] Host lock acquired by instance ID '00000000-0000-0000-0000-000000000000'.
[2024-01-15T10:30:46.456Z] ProcessOrder function triggered. Request received at 1/15/2024 10:30:46 AM
[2024-01-15T10:30:46.789Z] Request body: {"orderId":"ORD-20241513-001",...}
[2024-01-15T10:30:47.012Z] Order ORD-20241513-001 passed validation. Processing for customer CUST-987...
[2024-01-15T10:30:47.345Z] Order ORD-20241513-001 processed successfully. Response sent.
```

---

## Cloud Deployment (Azure Resources)

### Deploy Azure Resources Using ARM Template

```powershell
# Login to Azure
az login

# Set your subscription
az account set --subscription "Your-Subscription-ID"

# Deploy resources
az deployment group create `
  --resource-group "YourResourceGroup" `
  --template-file "docs/LogicApp-Template.json" `
  --parameters serviceBusNamespace="order-integration-sb" `
  --parameters functionAppName="order-process-func" `
  --parameters appInsightsName="order-insights"
```

### Deploy Azure Function

```powershell
# Publish function app to Azure
func azure functionapp publish "YourFunctionAppName"

# Or using dotnet CLI
dotnet publish -c Release -o publish
# Then deploy the publish folder through Azure Portal or VS
```

### Configure Logic App

1. In Azure Portal, navigate to your Logic App
2. Select **Logic App Designer**
3. Import the definition from `docs/LogicApp-Definition.json`
4. Configure connections:
   - Service Bus connection
   - Application Insights connection
   - HTTP endpoint (Azure Function URL)
5. Save and enable the workflow

---

## Troubleshooting

### Issue: Function App Won't Start

**Solution:**
```powershell
# Clear the local cache
Remove-Item -Recurse -Force @"
"$env:USERPROFILE\.azure\functions"

# Reinstall Azure Functions Core Tools
npm uninstall -g azure-functions-core-tools
npm install -g azure-functions-core-tools@4
```

### Issue: Port 7071 is Already in Use

**Solution:**
```powershell
# Find process using port 7071
netstat -ano | findstr :7071

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F

# Or use a different port
func start --port 7072
```

### Issue: "Unable to resolve JSON schema"

**Solution:**
- Ensure `local.settings.json` has correct syntax (valid JSON)
- Verify all required fields are present
- Re-open Visual Studio to refresh IntelliSense

### Issue: Function Returns 500 Internal Server Error

**Check logs:**
1. Look at console output for the error message
2. Common causes:
   - Missing dependency injection registration
   - Null reference exception
   - Deserialization error (wrong JSON format)

### Issue: Application Insights Not Recording Logs

**Solution:**
1. Set a valid instrumentation key in `local.settings.json`
2. Or create a new Application Insights instance in Azure Portal
3. Copy the instrumentation key and update the setting

---

## Performance Considerations

### Scalability
- **Functions**: Auto-scales based on demand (pay-per-use)
- **Service Bus**: Use Standard or Premium tier for production
- **Logic Apps**: Consumption plan scales automatically

### Optimization Tips
1. Enable Application Insights for performance monitoring
2. Use Service Bus batching for high-volume scenarios
3. Implement retry policies in Logic App
4. Cache frequently accessed data

---

## Security Best Practices

1. **Never commit connection strings** to version control
2. **Use Azure Key Vault** for sensitive data
3. **Enable Managed Identities** for Azure resource authentication
4. **Configure firewall rules** on Service Bus
5. **Implement authorization** on Azure Functions (not just Http trigger)
6. **Encrypt sensitive data** in transit and at rest

---

## Future Enhancements

- [ ] Add database integration (SQL Server / CosmosDB)
- [ ] Implement order status tracking
- [ ] Add email notifications for order confirmations
- [ ] Integrate with payment gateways
- [ ] Create admin dashboard for monitoring orders
- [ ] Add data export to data lake for analytics
- [ ] Implement message deduplication
- [ ] Add custom authentication/authorization
- [ ] Create unit tests for validation logic
- [ ] Implement circuit breaker pattern for resilience

---

## Support and Documentation

### Microsoft Documentation
- [Azure Functions Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/)
- [Azure Service Bus Documentation](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
- [Azure Logic Apps Documentation](https://learn.microsoft.com/en-us/azure/logic-apps/)
- [Application Insights Documentation](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

### Related Tutorials
- [Create a function app in Azure](https://learn.microsoft.com/en-us/azure/azure-functions/functions-create-first-azure-function)
- [Get started with Service Bus queues](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-portal)
- [Create automated workflows with Logic Apps](https://learn.microsoft.com/en-us/azure/logic-apps/quickstart-create-first-logic-app-workflow)

---

## License

This Proof of Concept is provided as-is for educational and demonstration purposes.

---

## Contact

For questions or issues related to this POC, please refer to the Azure documentation or contact your development team.

---

**Last Updated**: January 15, 2024
**Version**: 1.0
