# OrderIntegrationPOC - Enterprise-Grade Order Processing Platform

## 📋 Overview

This is a comprehensive enterprise-grade Order Processing Platform built with .NET 8 and Azure cloud services. It demonstrates modern cloud architecture patterns, security best practices, and production-ready implementation for order management systems.

### Core Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                   ERP/External Systems                          │
└────────────┬──────────────────────────┬───────────────────────┘
			 │                          │
	  ┌──────▼──────────┐       ┌──────▼──────────┐
	  │  Service Bus    │       │ Queue Storage   │
	  │ orders-incoming │       │  orders-queue   │
	  └──────┬──────────┘       └──────┬──────────┘
			 │                        │
	  ┌──────▼────────────────────────▼───────┐
	  │   Azure Functions (.NET 8)            │
	  │  ┌─────────────────────────────────┐  │
	  │  │ ServiceBusOrderIngestion        │  │
	  │  │ (Enterprise ERP Orders)         │  │
	  │  └─────────────────────────────────┘  │
	  │  ┌─────────────────────────────────┐  │
	  │  │ ProcessOrderToSql               │  │
	  │  │ (Direct Storage Processing)     │  │
	  │  └─────────────────────────────────┘  │
	  └──────┬──────────────────────────┬─────┘
			 │                          │
			 └──────────┬───────────────┘
						│
			┌───────────▼──────────────┐
			│  Azure SQL Database      │
			│ OrderIntegrationPOC_DB   │
			│  ┌──────────────────┐    │
			│  │ Customers        │    │
			│  │ Orders           │    │
			│  │ OrderLines       │    │
			│  └──────────────────┘    │
			└──────────────┬───────────┘
						   │
		┌──────────────────┴──────────────────┐
		│                                     │
	┌───▼────────────┐          ┌───────────▼──┐
	│ Logic Apps     │          │ Application  │
	│ Notifications  │          │ Insights     │
	└────────────────┘          │ Monitoring   │
								└──────────────┘
```

## 🎯 Key Features

### Domain Model Enhancements
- **Extended Entities**: Customer, OrderLine with complete relationships
- **Status Management**: OrderStatus enum for state tracking (Pending → Completed)
- **Audit Trail**: CreatedAt/UpdatedAt timestamps on all entities
- **Precision Pricing**: Decimal(10,2) for accurate financial calculations

### Data Integration
- **ERP Mapping**: ERPOrderDTO/ERPOrderLineDTO for external system integration
- **AutoMapper Profiles**: Automatic entity-DTO transformation
- **Validation Layer**: FluentValidation for all business rule enforcement
- **Repository Pattern**: IOrderRepository, ICustomerRepository, IOrderLineRepository

### Reliability & Resilience
- **Retry Policy**: Exponential backoff (1s → 32s max) for transient failures
- **Dead Letter Queue (DLQ)**: Poison message handling with metadata capture
- **Error Categories**: Custom exceptions for validation vs. processing errors
- **Host Configuration**: Service Bus extension, Queue visibility timeout, max dequeue count

### Azure Integration
- **Service Bus Trigger**: ServiceBusOrderIngestion function for async processing
- **Queue Storage**: Direct processing with storage validation
- **Application Insights**: Comprehensive telemetry and monitoring
- **Key Vault**: Managed identity-based secrets (production-ready)

### Security First
- **Managed Identity**: No connection strings in code or config
- **Private Endpoints**: Secure database connectivity
- **TLS 1.2+**: Enforced on all connections
- **RBAC**: Fine-grained access control via Azure roles
- **Audit Logging**: Complete compliance trail

### Testing Framework
- **Unit Tests**: xUnit with Moq, FluentAssertions
  - Validation testing (ERPOrderDTOValidatorTests)
  - Repository operations (OrderRepositoryTests)
  - In-memory database testing
- **Integration Tests**: End-to-end scenarios
  - Order processing pipeline (OrderProcessingPipelineTests)
  - Mock data fixtures for test scenarios
  - Batch operations, customer relationships, idempotency

## 📂 Project Structure

```
OrderIntegrationPOC/
├── OrderFunctionApp/                    # Main Azure Functions project
│   ├── Models/                          # Domain entities
│   │   ├── Order.cs                     # Updated with relationships
│   │   ├── Customer.cs                  # New: Customer entity
│   │   ├── OrderLine.cs                 # New: Line items
│   │   ├── OrderStatus.cs               # New: Status enum
│   │   ├── DTOs/                        # Data Transfer Objects
│   │   │   ├── ERPOrderDTO.cs
│   │   │   ├── OrderDTO.cs
│   │   │   └── CustomerDTO.cs
│   │   └── Mappings/
│   │       └── OrderMappingProfile.cs
│   │
│   ├── Data/                            # Data access layer
│   │   ├── OrderIntegrationContext.cs   # Updated DbContext
│   │   └── Repositories/                # New: Repository pattern
│   │       ├── IOrderRepository.cs
│   │       ├── ICustomerRepository.cs
│   │       ├── OrderRepository.cs
│   │       └── CustomerRepository.cs
│   │
│   ├── Functions/                       # Azure Functions
│   │   ├── ServiceBusOrderIngestion.cs  # New: Service Bus trigger
│   │   ├── ProcessOrderToSql.cs         # Updated with validation
│   │   └── OrderProcessor.cs            # Queue processing
│   │
│   ├── Services/                        # Business services
│   │   ├── RetryPolicy.cs               # New: Retry logic
│   │   └── PoisonMessageHandler.cs      # New: DLQ handling
│   │
│   ├── Validation/                      # New: Validation rules
│   │   ├── ERPOrderDTOValidator.cs
│   │   ├── OrderValidator.cs
│   │   └── CustomerValidator.cs
│   │
│   ├── Exceptions/                      # New: Custom exceptions
│   │   ├── OrderValidationException.cs
│   │   └── OrderProcessingException.cs
│   │
│   ├── Program.cs                       # Updated with DI registration
│   ├── host.json                        # Updated with DLQ config
│   └── OrderFunctionApp.csproj          # Updated dependencies
│
├── OrderFunctionApp.Tests/              # New: Unit tests
│   ├── Validation/
│   │   └── ERPOrderDTOValidatorTests.cs
│   └── Repositories/
│       └── OrderRepositoryTests.cs
│
├── OrderFunctionApp.IntegrationTests/   # New: Integration tests
│   ├── Fixtures/
│   │   └── MockOrderData.cs
│   └── OrderProcessingPipelineTests.cs
│
├── bicep/                               # New: Infrastructure as Code
│   ├── main.bicep                       # Orchestration template
│   ├── key-vault.bicep                  # Key Vault deployment
│   ├── apim.bicep                       # API Management
│   ├── private-endpoint.bicep           # Private endpoint config
│   └── INFRASTRUCTURE.md                # Deployment guide
│
├── logicapps/                           # Logic Apps workflows
│   ├── order-notification.json
│   ├── deploy.bicep
│   └── DEPLOY.md
│
├── SECURITY.md                          # New: Security hardening guide
└── README.md                            # This file
```

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022+ or VS Code
- Azure CLI 2.20+ (for cloud deployment)
- SQL Server (local or Azure)
- Azurite for local testing (optional)

### Local Development

1. **Clone and Setup**
   ```powershell
   git clone https://github.com/ttkiviranta/OrderIntegrationPOC.git
   cd OrderIntegrationPOC
   dotnet build
   ```

2. **Configure Database**
   ```powershell
   cd OrderFunctionApp
   dotnet ef database update
   ```

3. **Run Azure Functions Locally**
   ```powershell
   func start
   ```

4. **Run Tests**
   ```powershell
   dotnet test OrderFunctionApp.Tests
   dotnet test OrderFunctionApp.IntegrationTests
   ```

### Azure Deployment

```powershell
# Deploy infrastructure
az deployment group create `
  --resource-group rg-orderintegrationpoc-prod `
  --template-file bicep/main.bicep `
  --parameters projectName=orderintegrationpoc environment=prod

# Deploy Function App
func azure functionapp publish orderfunc-prod
```

See [INFRASTRUCTURE.md](bicep/INFRASTRUCTURE.md) for detailed deployment steps.

## 🔒 Security Configuration

The solution implements comprehensive security best practices:

### In Development
- Local Key Vault simulation (optional)
- Direct database connections with Trusted Connection
- Test-oriented configurations

### In Production
✅ Key Vault management of all secrets  
✅ Managed Identity authentication  
✅ Private Endpoints for database/storage  
✅ TLS 1.2+ enforced  
✅ APIM with OAuth 2.0  
✅ Audit logging enabled  
✅ Network segmentation  
✅ Role-based access control  

See [SECURITY.md](SECURITY.md) for step-by-step hardening guide.

## 📊 Recent Enhancements (12-Step Improvement Plan)

### Step 1-3: Domain & Repository Pattern ✅
- Extended domain models (OrderLine, Customer, OrderStatus)
- Created comprehensive DTOs for ERP integration
- Implemented repository pattern with EF Core

### Step 4: Error Handling & DLQ ✅
- Poison message handler with metadata capture
- Retry policy with exponential backoff
- Updated host.json for DLQ/Service Bus configuration

### Step 5: Service Bus Ingestion ✅
- New ServiceBusOrderIngestion function
- Complete ERP order processing pipeline
- Validation, customer management, status tracking

### Step 6: Validation Framework ✅
- FluentValidation for all entity types
- Business rule enforcement
- Comprehensive error messages

### Step 7: Production Configuration ✅
- Key Vault integration
- Managed Identity support
- Complete DI container setup

### Step 8: Infrastructure as Code ✅
- Bicep templates for all Azure resources
- Key Vault, API Management, Private Endpoints
- Deployment guides and scripts

### Step 9-10: Testing ✅
- xUnit unit tests with mocks
- Integration tests with in-memory database
- Mock data fixtures

### Step 11: Security Documentation ✅
- Comprehensive SECURITY.md guide
- Step-by-step hardening procedures
- Compliance checklist (80+ items)

### Step 12: Build Validation ✅
- Solution builds successfully
- All projects verified
- Dependencies validated

## 🔄 API Endpoints

### Direct Insert (Queue Storage)
```
POST /api/orders/direct

Request:
{
  "orderId": "ORD-2024-001",
  "customerId": "CUST-001",
  "total": 299.99,
  "orderDate": "2024-01-01T00:00:00Z"
}

Response:
{
  "success": true,
  "message": "Order inserted successfully",
  "orderId": "ORD-2024-001",
  "rowsAffected": 1
}
```

### Service Bus Ingestion
```
POST /api/orders

Request:
{
  "externalOrderId": "ERP-001",
  "externalCustomerId": "ERP-CUST-001",
  "customerName": "Acme Corp",
  "customerEmail": "contact@acme.com",
  "total": 5299.99,
  "orderDate": "2024-01-01T00:00:00Z",
  "lineItems": [
	{
	  "productId": "PROD-001",
	  "description": "Laptop",
	  "quantity": 2,
	  "unitPrice": 2500.00,
	  "lineTotal": 5000.00
	}
  ]
}

Response:
{
  "success": true,
  "message": "Order queued for processing (Service Bus)"
}
```

## 📈 Monitoring & Observability

### Application Insights
- Function execution metrics
- Custom telemetry events
- Exception tracking
- Dependency monitoring

### Log Analytics
- Query templates provided
- Performance baselines
- Alert rules configured
- Diagnostic settings enabled

### Key Metrics
- Order processing latency
- Success/failure rates
- Queue depth and throughput
- Database response times

## 🧪 Testing Strategy

### Unit Tests
```powershell
dotnet test OrderFunctionApp.Tests --logger "console;verbosity=detailed"
```

Tests cover:
- Order validation rules
- Customer CRUD operations
- Status transitions
- Date range queries

### Integration Tests
```powershell
dotnet test OrderFunctionApp.IntegrationTests
```

Tests cover:
- End-to-end order pipeline
- Customer relationship persistence
- Batch order processing
- Idempotency guarantees
- Concurrency scenarios

## 📚 Documentation

- **[SECURITY.md](SECURITY.md)** - Security hardening guide
- **[INFRASTRUCTURE.md](bicep/INFRASTRUCTURE.md)** - Deployment procedures
- **[EF_CORE_INTEGRATION.md](OrderFunctionApp/README_EFCORE_INTEGRATION.md)** - Database patterns
- **[DEPLOY.md](logicapps/DEPLOY.md)** - Logic Apps deployment

## 🔗 Dependencies

### NuGet Packages
```
Microsoft.Azure.Functions.Worker (2.51.0)
Microsoft.EntityFrameworkCore (8.0.0)
AutoMapper (13.0.1)
FluentValidation (11.9.2)
Azure.Identity (1.17.0)
Azure.Storage.Queues (12.21.0)
```

### Azure Services
- Azure Functions (Consumption Plan)
- Azure SQL Database
- Azure Service Bus
- Azure Application Insights
- Azure Key Vault
- Azure Logic Apps

## 🤝 Contributing

1. Create feature branch: `git checkout -b feature/your-feature`
2. Commit with clear messages
3. Push and create Pull Request
4. Ensure all tests pass

## 📝 Commit History

Recent commits include:
- `feat: extend domain models with OrderLine, Customer, OrderStatus`
- `refactor: update database context and DI configuration`
- `test: add unit and integration test suites`
- `docs: add infrastructure and security documentation`

## ⚖️ License

[Specify your license]

## 📧 Contact

For questions or support, contact: [your contact info]

---

**Last Updated**: January 2025  
**Version**: 2.0 (Enterprise Edition)  
**Status**: Production Ready ✅
