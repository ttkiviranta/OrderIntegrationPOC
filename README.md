# Order Integration Proof of Concept (POC)

## Quick Links

- **🚀 Quick Start**: Read [QUICKSTART.md](OrderIntegrationPOC/QUICKSTART.md) (3 minutes)
- **📖 Full Documentation**: See [README.md](OrderIntegrationPOC/README.md)
- **🧪 Testing Guide**: Check [Testing-Guide.md](OrderIntegrationPOC/docs/Testing-Guide.md)
- **☁️ Deployment Guide**: Review [Deployment-Guide.md](OrderIntegrationPOC/docs/Deployment-Guide.md)
- **API Reference**: See [API-Reference.md](OrderIntegrationPOC/docs/API-Reference.md)
- **Architecture**: View [Architecture-Diagram.md](OrderIntegrationPOC/docs/Architecture-Diagram.md)

---

## 📋 Overview

This Proof of Concept demonstrates a complete end-to-end order processing pipeline integrating:

- **Azure Service Bus** - Asynchronous message queuing
- **Azure Logic Apps** - Workflow orchestration  
- **Azure Functions** - Serverless order processing (C#, .NET 8)
- **Azure SQL Database** - Persistent order storage *(NEW)*
- **Application Insights** - Monitoring and telemetry

### Business Scenario

An ERP system simulates sending customer orders to both an Azure Service Bus queue (for workflow orchestration) and an Azure Queue Storage (for direct database persistence). A Logic App automatically triggers when orders arrive via Service Bus, validates the payload, calls an Azure Function for processing, and logs all activities to Application Insights. Simultaneously, the ProcessOrderToSql Queue-triggered Function processes orders from the queue and persists them directly to Azure SQL Database for long-term storage.

### What's New: SQL Persistence Layer (v2.0)

✨ **NEW in v2.0:**
- **Direct SQL Persistence** - Queue-triggered function that automatically persists orders to Azure SQL Database
- **Orders Table** - Structured data storage with audit trail and performance indexes
- **Async Operations** - Non-blocking database operations using async/await patterns
- **Parameterized Queries** - SQL injection protection with parameterized statements
- **Complete Documentation** - 1700+ lines of comprehensive guides and examples

---

## 🏗️ Architecture

### Classic Pipeline (Service Bus + Logic App + HTTP Function)
```
ERP System
	↓ (JSON Order)
Azure Service Bus (orders-incoming queue)
	↓ (Message Trigger)
Azure Logic App (OrderProcessingWorkflow)
	├─ Parse JSON
	├─ Validate Structure
	├─ Call ProcessOrder Function → [HTTP POST]
	└─ Log Results to Application Insights
		 ↓
	Azure Function: ProcessOrder
	├─ Deserialize JSON
	├─ Validate Order Data
	├─ Log Results
	└─ Return HTTP 200/400
		 ↓
	Application Insights (Monitoring & Telemetry)
```

### NEW: Direct SQL Persistence Pipeline (v2.0)
```
ERP System / Queue Message
	↓ (JSON Order)
Azure Queue Storage (orders-queue)
	↓ (Queue Message Trigger)
Azure Function: ProcessOrderToSql
	├─ Deserialize OrderRequest
	├─ Open async SQL connection
	├─ Execute parameterized INSERT
	├─ Handle errors gracefully
	└─ Log success/errors to Application Insights
		 ↓
	Azure SQL Database (OrderIntegrationPOC_DB)
		 ↓
	Orders Table
	├─ Id (IDENTITY PRIMARY KEY)
	├─ OrderId (UNIQUE, indexed)
	├─ CustomerId (indexed)
	├─ Total (decimal)
	├─ Description (optional)
	├─ OrderDate
	└─ CreatedAt (audit trail)
		 ↓
	Application Insights Telemetry
```

---

## 🚀 Getting Started (3 Minutes)

### 1. Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools v4+
- Visual Studio 2022 or VS Code

### 2. Clone & Open
```powershell
git clone https://github.com/ttkiviranta/OrderIntegrationPOC.git
cd OrderIntegrationPOC
```

### 3. Start Locally
```powershell
cd OrderIntegrationPOC/OrderFunctionApp
func start
```

### 4. Test (in another terminal)
```powershell
$body = @{
	orderId = "TEST-001"
	customerId = "CUST-001"
	total = 99.99
} | ConvertTo-Json

Invoke-RestMethod `
	-Uri "http://localhost:7071/api/orders/process" `
	-Method Post `
	-ContentType "application/json" `
	-Body $body
```

**Expected Response:**
```json
{
  "status": "success",
  "message": "Order processed successfully",
  "orderId": "TEST-001",
  "customerId": "CUST-001",
  "total": 99.99,
  "processedAt": "2024-01-15T10:30:45.123Z",
  "environment": "Local Development"
}
```

---

## 📁 Solution Structure

```
OrderIntegrationPOC/
│
├── OrderFunctionApp/                      # Azure Functions Project (.NET 8)
│   ├── Functions/
│   │   └── ProcessOrder.cs               # HTTP-triggered order processing function
│   │
│   ├── Models/
│   │   ├── OrderRequest.cs               # Order input model
│   │   └── OrderValidationResult.cs      # Validation result model
│   │
│   ├── Services/
│   │   └── OrderValidator.cs             # Business validation logic
│   │
│   ├── Program.cs                        # DI & Application Insights setup
│   ├── host.json                         # Function runtime configuration
│   ├── local.settings.json               # Local development settings
│   └── OrderFunctionApp.csproj           # Project file
│
├── docs/                                  # Documentation & Templates
│   ├── QUICKSTART.md                     # 3-minute quick start guide
│   ├── README.md                         # Detailed documentation
│   ├── Testing-Guide.md                  # 7 test scenarios
│   ├── API-Reference.md                  # API documentation
│   ├── Architecture-Diagram.md           # System architecture diagrams
│   ├── Deployment-Guide.md               # Azure deployment instructions
│   ├── LogicApp-Template.json            # ARM template
│   ├── LogicApp-Definition.json          # Logic App workflow
│   └── sample-payloads.json              # Example test data
│
├── README.md                             # This file (repo root)
├── OrderIntegrationPOC.slnx              # Visual Studio solution
└── .gitignore                            # Git ignore rules
```

---

## ✅ Features

- ✅ HTTP-triggered Azure Function
- ✅ Queue-triggered Azure Function with SQL persistence ⭐ NEW
- ✅ Azure SQL Database integration with async operations ⭐ NEW
- ✅ Orders table with performance indexes and audit trail ⭐ NEW
- ✅ Parameterized SQL queries for injection protection ⭐ NEW
- ✅ JSON payload validation (orderId, customerId, total)
- ✅ Comprehensive error handling
- ✅ Application Insights integration
- ✅ Service Bus message queue support
- ✅ Queue Storage with direct database persistence ⭐ NEW
- ✅ Logic App orchestration templates
- ✅ Detailed logging throughout the pipeline
- ✅ Local development configuration
- ✅ Production-ready deployment guide
- ✅ Complete test suite documentation
- ✅ 1700+ lines of SQL persistence documentation ⭐ NEW

---

## 📚 Documentation

| Document | Purpose |
|----------|---------| 
| [QUICKSTART.md](OrderIntegrationPOC/QUICKSTART.md) | Get running in 3 minutes |
| [README.md](OrderIntegrationPOC/README.md) | Full detailed documentation |
| [Testing-Guide.md](OrderIntegrationPOC/docs/Testing-Guide.md) | 7 test scenarios + debugging |
| [API-Reference.md](OrderIntegrationPOC/docs/API-Reference.md) | API endpoint documentation |
| [Architecture-Diagram.md](OrderIntegrationPOC/docs/Architecture-Diagram.md) | System architecture & diagrams |
| [Deployment-Guide.md](OrderIntegrationPOC/docs/Deployment-Guide.md) | Azure cloud deployment steps |
| **[START_HERE.md](START_HERE.md)** | **SQL persistence quick start** ⭐ NEW |
| **[TESTING_GUIDE.md](TESTING_GUIDE.md)** | **SQL testing guide (8 parts)** ⭐ NEW |
| **[INDEX.md](INDEX.md)** | **Complete navigation guide** ⭐ NEW |

---

## 🧪 Testing

The solution includes comprehensive test scenarios:

1. ✅ **Valid Order** - Successful processing
2. ❌ **Missing CustomerId** - Validation error
3. ❌ **Invalid Total** - Business rule violation
4. ❌ **Malformed JSON** - Format error
5. **Batch Testing** - Multiple orders
6. **Performance Testing** - Load testing
7. **Visual Studio Testing** - Using REST Client extension

See [Testing-Guide.md](OrderIntegrationPOC/docs/Testing-Guide.md) for detailed instructions.

---

## ☁️ Deployment to Azure

### Prerequisites
- Azure subscription
- Azure CLI installed

### Quick Deploy
```powershell
az login
cd OrderIntegrationPOC/OrderFunctionApp
func azure functionapp publish "your-function-app-name"
```

See [Deployment-Guide.md](OrderIntegrationPOC/docs/Deployment-Guide.md) for complete instructions.

---

## 🔧 Configuration

### Local Development (local.settings.json)
```json
{
	"IsEncrypted": false,
	"Values": {
		"AzureWebJobsStorage": "UseDevelopmentStorage=true",
		"FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
		"APPINSIGHTS_INSTRUMENTATIONKEY": "YOUR_KEY_HERE",
		"ENVIRONMENT": "Development"
	}
}
```

### Production Settings (Azure Portal)
- Set Application Insights instrumentation key
- Configure Service Bus connection string
- Set appropriate environment variables
- Enable diagnostics logging

---

## 🛠️ Technology Stack

- **Language**: C# 12 with .NET 8.0
- **Runtime**: Azure Functions (dotnet-isolated model)
- **Message Queue**: Azure Service Bus
- **Orchestration**: Azure Logic Apps
- **Monitoring**: Application Insights
- **Infrastructure**: ARM Templates
- **DevOps**: Git + GitHub

---

## 📊 Request/Response Examples

### Success Response (HTTP 200)
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

### Error Response (HTTP 400)
```json
{
  "status": "error",
  "message": "Field 'customerId' is required and cannot be empty.",
  "timestamp": "2024-01-15T10:31:00.5678901Z",
  "environment": "Local Development"
}
```

---

## 🔒 Security Best Practices

- ❌ Never commit connection strings to version control
- ✅ Use Azure Key Vault for sensitive data
- ✅ Enable Managed Identities for Azure resources
- ✅ Implement proper authentication
- ✅ Use HTTPS for all communications
- ✅ Encrypt data in transit and at rest

---

## 🚨 Troubleshooting

### Port 7071 Already in Use
```powershell
netstat -ano | findstr :7071
taskkill /PID <PID> /F
```

### Functions Worker Runtime Not Found
```powershell
npm uninstall -g azure-functions-core-tools
npm install -g azure-functions-core-tools@4
```

### Build Errors
```powershell
# Restore packages
dotnet restore

# Clean build
dotnet clean
dotnet build
```

See [Testing-Guide.md](OrderIntegrationPOC/docs/Testing-Guide.md) for more troubleshooting.

---

## 💡 Future Enhancements

- [ ] ~Database integration (SQL Server / CosmosDB)~ ✅ DONE - SQL Server integration complete (v2.0)
- [ ] Order status tracking
- [ ] Email notifications
- [ ] Payment gateway integration
- [ ] Admin dashboard
- [ ] Data export functionality
- [ ] Message deduplication
- [ ] Custom authentication schemes
- [ ] Unit and integration tests
- [ ] Circuit breaker pattern

---

## 🆕 SQL Persistence Features (v2.0)

### What's New

Added comprehensive SQL persistence layer for direct order storage in Azure SQL Database:

#### ProcessOrderToSql Azure Function
- **Trigger Type**: Queue-triggered (Azure Queue Storage - `orders-queue`)
- **Functionality**:
  - Deserializes JSON queue messages into OrderRequest objects
  - Opens async SQL connection using SqlConnectionString from configuration
  - Executes parameterized INSERT statements (prevents SQL injection)
  - Logs success and error scenarios to Application Insights
  - Handles missing/null values gracefully with defaults

#### Orders Table Schema
```sql
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId NVARCHAR(50) NOT NULL UNIQUE,
    CustomerId NVARCHAR(50) NOT NULL,
    Total DECIMAL(10,2) NOT NULL,
    Description NVARCHAR(255),
    OrderDate DATETIME NOT NULL,
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

-- Performance Indexes
CREATE INDEX IX_Orders_OrderId ON Orders(OrderId);
CREATE INDEX IX_Orders_CustomerId ON Orders(CustomerId);
CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);
```

#### Dependencies Added
- `Microsoft.Data.SqlClient` v5.1.5 - SQL Server connectivity
- `Azure.Storage.Queues` v12.21.0 - Queue storage integration
- `Microsoft.Azure.Functions.Worker.Extensions.Storage.Queues` v5.5.4 - Queue triggers

#### Configuration
Add to `local.settings.json`:
```json
{
  "SqlConnectionString": "Server=tcp:{server}.database.windows.net,1433;Initial Catalog={database};User ID={user};Password={password};..."
}
```

#### Documentation
Comprehensive guides included:
- **START_HERE.md** - 5-minute quick start
- **TESTING_GUIDE.md** - 8-part testing guide (400+ lines)
- **SQL_PERSISTENCE_SUMMARY.md** - Technical implementation details
- **QUICK_REFERENCE.md** - Fast lookup and troubleshooting
- **INDEX.md** - Complete navigation guide

### Testing SQL Features

#### Quick Test
1. Create Orders table: Execute `OrderFunctionApp/SQL/CreateOrdersTable.sql`
2. Start Functions: `func start`
3. Send queue message via Azure Storage Explorer
4. Verify in SQL: `SELECT * FROM Orders;`

See **[TESTING_GUIDE.md](TESTING_GUIDE.md)** for complete testing procedures.

### Performance & Security

✅ **Performance Optimized**
- Indexed columns for fast queries (OrderId, CustomerId, OrderDate)
- Async/await patterns throughout
- Connection pooling enabled
- Audit trail with CreatedAt timestamp

✅ **Security Hardened**
- Parameterized queries (prevents SQL injection)
- Encrypted SQL connections
- IConfiguration-based secret management
- No credential exposure in logs

---

## 📖 Learning Resources

- [Azure Functions Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/)
- [Azure Service Bus Documentation](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
- [Azure Logic Apps Documentation](https://learn.microsoft.com/en-us/azure/logic-apps/)
- [Application Insights Documentation](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

---

## 👤 Author

**Timo Kiviranta**  
GitHub: [@ttkiviranta](https://github.com/ttkiviranta)

---

## ⚠️ Known Issues: Swagger/OpenAPI Support

### Issue: Swagger UI Not Available

**Status**: Acknowledged limitation of the .NET 8 Isolated Worker runtime

**Description**:
Swagger UI documentation is not available in the current implementation due to compatibility issues with the .NET 8 Azure Functions Isolated Worker model.

**Root Cause**:
- Microsoft's `Microsoft.Azure.WebJobs.Extensions.OpenApi` package has limited/unstable support for .NET 8 Isolated Workers
- Preview versions (e.g., v2.0.0-preview2) cause dependency registration errors during startup
- Stable versions (e.g., v1.5.0) do not include the required types:
  - `IOpenApiHttpTriggerContext`
  - `OpenApiHttpTriggerContext`
  - `IOpenApiTriggerFunction`
  - `OpenApiTriggerFunction`
- No official Microsoft solution exists yet for this specific runtime configuration

**Workarounds**:

1. **Use Postman (Recommended)**:
   - Import the ReceiveOrder endpoint manually
   - Test with JSON body samples provided in the [Testing-Guide.md](OrderIntegrationPOC/docs/Testing-Guide.md)

2. **Use cURL or PowerShell**:
   ```powershell
   $body = @{
       orderId = "TEST-001"
       customerId = "CUST-001"
       total = 99.99
       description = "Test order"
   } | ConvertTo-Json

   Invoke-WebRequest -Uri "http://localhost:7071/api/orders" `
     -Method POST `
     -Headers @{"Content-Type"="application/json"} `
     -Body $body
   ```

3. **Use VS Code REST Client Extension**:
   - Install the "REST Client" extension
   - Create a `.http` or `.rest` file with API requests
   - Execute requests directly from the editor

4. **Manual OpenAPI Specification**:
   - Create a static `openapi.json` file
   - Use third-party Swagger UI to render the specification
   - See [API-Reference.md](OrderIntegrationPOC/docs/API-Reference.md) for endpoint details

**What This Means**:
- ✅ The API endpoints function correctly
- ✅ All validation and processing works as expected
- ✅ Can be deployed to Azure without issues
- ❌ No automatic Swagger documentation UI
- ❌ Manual testing required (Postman/cURL)

**Future Resolution**:
As Microsoft releases stable OpenAPI support for .NET 8 Isolated Workers, this issue will be resolved. Monitor the [Azure Functions roadmap](https://github.com/Azure/azure-functions-host) for updates.

**References**:
- [Azure Functions OpenAPI Extension](https://github.com/Azure/azure-functions-openapi-extension)
- [.NET 8 Isolated Worker Limitations](https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection)

---

This Proof of Concept is provided as-is for educational and demonstration purposes.

---

## 🤝 Contributing

Feel free to fork this repository and submit pull requests for any improvements.

---

**Last Updated**: July 8, 2026  
**Version**: 2.0 - SQL Persistence Edition  
**Status**: ✅ Complete and Ready for Use
