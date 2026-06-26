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
- **Application Insights** - Monitoring and telemetry

### Business Scenario

An ERP system simulates sending customer orders to an Azure Service Bus queue. A Logic App automatically triggers when orders arrive, validates the payload, calls an Azure Function for processing, and logs all activities to Application Insights.

---

## 🏗️ Architecture

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
- ✅ JSON payload validation (orderId, customerId, total)
- ✅ Comprehensive error handling
- ✅ Application Insights integration
- ✅ Service Bus message queue support
- ✅ Logic App orchestration templates
- ✅ Detailed logging throughout the pipeline
- ✅ Local development configuration
- ✅ Production-ready deployment guide
- ✅ Complete test suite documentation

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

- [ ] Database integration (SQL Server / CosmosDB)
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

## 📝 License

This Proof of Concept is provided as-is for educational and demonstration purposes.

---

## 🤝 Contributing

Feel free to fork this repository and submit pull requests for any improvements.

---

**Last Updated**: January 15, 2024  
**Version**: 1.0  
**Status**: ✅ Complete and Ready for Use
