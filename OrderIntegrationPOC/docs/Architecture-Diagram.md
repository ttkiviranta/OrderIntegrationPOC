# Order Integration POC - Architecture and Data Flow Diagrams

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                        ORDER INTEGRATION POC ARCHITECTURE                        │
└─────────────────────────────────────────────────────────────────────────────────┘

	┌──────────────────┐
	│  ERP System      │
	│                  │
	│ Simulates order  │
	│ messages (JSON)  │
	└────────┬─────────┘
			 │
			 │ POST JSON Order
			 │
	┌────────▼──────────────────────────────────────────────────────────────┐
	│          AZURE SERVICE BUS (orders-incoming Queue)                     │
	│                                                                        │
	│  ┌────────────────────────────────────────────────────────────────┐  │
	│  │ Message:                                                       │  │
	│  │ {                                                              │  │
	│  │   "orderId": "ORD-20241513-001",                             │  │
	│  │   "customerId": "CUST-987",                                  │  │
	│  │   "total": 149.90                                            │  │
	│  │ }                                                              │  │
	│  └────────────────────────────────────────────────────────────────┘  │
	└────────┬──────────────────────────────────────────────────────────────┘
			 │
			 │ Message Arrival (Polling/Trigger)
			 │
	┌────────▼──────────────────────────────────────────────────────────────┐
	│     AZURE LOGIC APP (OrderProcessingWorkflow)                          │
	│                                                                        │
	│  Trigger: Service Bus Message Received (orders-incoming)             │
	│           ↓                                                           │
	│  Step 1: Parse JSON Message                                          │
	│           │                                                          │
	│           ├─ Parse using JSON schema                                │
	│           └─ Extract: orderId, customerId, total                    │
	│           ↓                                                          │
	│  Step 2: Validate Message Structure                                  │
	│           │                                                          │
	│           ├─ Required fields present?                               │
	│           └─ Format valid?                                          │
	│           ↓                                                          │
	│  Step 3: Call Azure Function (HTTP POST)                            │
	│           │                                                          │
	│           └─ Send JSON payload to ProcessOrder Function             │
	│           ↓                                                          │
	│  Step 4: Handle Response                                             │
	│           │                                                          │
	│           ├─ Success (HTTP 200) → Log Success                       │
	│           └─ Error (HTTP 400) → Log Error                           │
	│           ↓                                                          │
	│  Step 5: Log to Application Insights                                 │
	│           │                                                          │
	│           └─ Telemetry event with status, duration, errors         │
	│                                                                     │
	└────────┬──────────────────────────────────────────────────────────────┘
			 │
			 │ HTTP POST /api/orders/process
			 │
	┌────────▼──────────────────────────────────────────────────────────────┐
	│   AZURE FUNCTION: ProcessOrder (HTTP-Triggered)                       │
	│                                                                        │
	│   Input: JSON Payload (OrderRequest)                                 │
	│   ┌──────────────────────────────────────────────────────────────┐  │
	│   │ Step 1: Deserialize JSON                                     │  │
	│   │   Input: Request body                                        │  │
	│   │   Output: OrderRequest object                                │  │
	│   └──────────────────────────────────────────────────────────────┘  │
	│                           ↓                                          │
	│   ┌──────────────────────────────────────────────────────────────┐  │
	│   │ Step 2: Validate Order (OrderValidator Service)             │  │
	│   │   - Check if order object is null                           │  │
	│   │   - Validate: orderId (non-empty)                           │  │
	│   │   - Validate: customerId (non-empty)                        │  │
	│   │   - Validate: total (> 0)                                   │  │
	│   │   Output: OrderValidationResult                             │  │
	│   └──────────────────────────────────────────────────────────────┘  │
	│                           ↓                                          │
	│   ┌──────────────────────────────────────────────────────────────┐  │
	│   │ Decision: Is Valid?                                          │  │
	│   │                                                              │  │
	│   │ ├─ YES → HTTP 200 OK (Success Response)                     │  │
	│   │ │        { status: "success", orderId: "ORD-...", ... }     │  │
	│   │ │                                                            │  │
	│   │ └─ NO  → HTTP 400 Bad Request (Error Response)              │  │
	│   │          { status: "error", message: "Field X required" }   │  │
	│   └──────────────────────────────────────────────────────────────┘  │
	│                           ↓                                          │
	│   ┌──────────────────────────────────────────────────────────────┐  │
	│   │ Step 3: Logging (Application Insights)                      │  │
	│   │   - Log function trigger event                              │  │
	│   │   - Log request body received                               │  │
	│   │   - Log validation steps                                    │  │
	│   │   - Log final status (success/error)                        │  │
	│   │   - Include order details and timestamps                    │  │
	│   └──────────────────────────────────────────────────────────────┘  │
	│                           ↓                                          │
	│   ┌──────────────────────────────────────────────────────────────┐  │
	│   │ Step 4: Return HTTP Response                                │  │
	│   │   Logic App receives response                               │  │
	│   │   Parses success/error information                          │  │
	│   └──────────────────────────────────────────────────────────────┘  │
	│                                                                        │
	└────────┬──────────────────────────────────────────────────────────────┘
			 │
			 │ Response Data (success/error)
			 │
	┌────────▼──────────────────────────────────────────────────────────────┐
	│      AZURE APPLICATION INSIGHTS                                        │
	│                                                                        │
	│  Telemetry Sources:                                                   │
	│  ├─ Function App Logs                                               │
	│  │  ├─ Function execution events                                    │
	│  │  ├─ Request/response metrics                                     │
	│  │  └─ Custom logging (validation, errors)                          │
	│  │                                                                   │
	│  ├─ Logic App Logs                                                  │
	│  │  ├─ Workflow run history                                         │
	│  │  ├─ Action outcomes                                              │
	│  │  ├─ Error handling events                                        │
	│  │  └─ Performance metrics                                          │
	│  │                                                                   │
	│  └─ Custom Events                                                   │
	│     ├─ Order processed (success/failure)                            │
	│     ├─ Validation results                                           │
	│     └─ Business metrics                                             │
	│                                                                        │
	│  Dashboard Views:                                                     │
	│  ├─ Request rates and latency                                        │
	│  ├─ Error rates and types                                            │
	│  ├─ Order processing trends                                          │
	│  └─ Custom metrics (per-customer, per-order)                         │
	│                                                                        │
	└────────────────────────────────────────────────────────────────────────┘
```

---

## Request/Response Flow Diagram

### Success Path (HTTP 200 OK)

```
Client Request
	│
	▼
┌─────────────────────────────────────────┐
│ ProcessOrder Function Receives Request  │
└─────────────────────────────────────────┘
	│
	▼
┌─────────────────────────────────────────────────┐
│ 1. Read Request Body (Stream Reader)            │
│    ↓                                            │
│    Body Content: {"orderId":"...", ...}        │
└─────────────────────────────────────────────────┘
	│
	▼
┌─────────────────────────────────────────────────┐
│ 2. Deserialize JSON (JsonConvert)               │
│    ✓ Result: OrderRequest object               │
│    × Exception: JSON parse error                │
└─────────────────────────────────────────────────┘
	│
	├─ Parse Error → HTTP 400 Bad Request
	│
	▼ (Success)
┌─────────────────────────────────────────────────┐
│ 3. Call OrderValidator Service                  │
│    → ValidateOrder(orderRequest)               │
└─────────────────────────────────────────────────┘
	│
	▼
┌─────────────────────────────────────────────────────────┐
│ 4. Validate Required Fields                            │
│                                                        │
│    Check 1: orderId non-empty?                        │
│    ├─ No → Error: "Field 'orderId' required"         │
│    └─ Yes → Continue                                  │
│                                                        │
│    Check 2: customerId non-empty?                     │
│    ├─ No → Error: "Field 'customerId' required"      │
│    └─ Yes → Continue                                  │
│                                                        │
│    Check 3: total > 0?                                │
│    ├─ No → Error: "Field 'total' must be > 0"        │
│    └─ Yes → Continue                                  │
│                                                        │
│    All checks passed → OrderValidationResult         │
│                        { IsValid: true, ... }         │
└─────────────────────────────────────────────────────────┘
	│
	├─ Validation Failed → HTTP 400 Bad Request
	│
	▼ (Validation Passed)
┌─────────────────────────────────────────────────┐
│ 5. Log Success (ILogger)                        │
│    "Order {Id} passed validation"               │
│    "Processing for customer {CustId}"           │
└─────────────────────────────────────────────────┘
	│
	▼
┌─────────────────────────────────────────────────┐
│ 6. Generate Success Response                    │
│    {                                            │
│      status: "success",                         │
│      message: "Order processed successfully",  │
│      orderId: "ORD-...",                        │
│      customerId: "CUST-...",                    │
│      total: 149.90,                             │
│      processedAt: "2024-01-15T10:30:45Z"       │
│    }                                            │
└─────────────────────────────────────────────────┘
	│
	▼
┌─────────────────────────────────────────────────┐
│ 7. Return HTTP 200 OK with JSON Response        │
└─────────────────────────────────────────────────┘
	│
	▼
Client Receives Success Response
```

---

### Error Path (HTTP 400 Bad Request)

```
Client Request
	│
	▼
ProcessOrder Function
	│
	├─ Validation Failed (orderId missing)
	│
	▼
┌─────────────────────────────────────────────────┐
│ OrderValidator returns:                         │
│ {                                               │
│   IsValid: false,                               │
│   ErrorMessage: "Field 'orderId' required..."  │
│ }                                               │
└─────────────────────────────────────────────────┘
	│
	▼
┌─────────────────────────────────────────────────┐
│ Log Warning                                     │
│ "Validation failed: OrderId missing"           │
└─────────────────────────────────────────────────┘
	│
	▼
┌─────────────────────────────────────────────────┐
│ Create Error Response                           │
│ {                                               │
│   status: "error",                              │
│   message: "Field 'orderId' required...",      │
│   timestamp: "2024-01-15T10:31:00Z"            │
│ }                                               │
└─────────────────────────────────────────────────┘
	│
	▼
┌─────────────────────────────────────────────────┐
│ Return HTTP 400 Bad Request                     │
└─────────────────────────────────────────────────┘
	│
	▼
Client Receives Error Response
	│
	├─ Parse error message
	└─ Retry with corrected data
```

---

## Data Model Diagram

```
┌──────────────────────────────────────────────────────┐
│            OrderRequest (Input Model)               │
├──────────────────────────────────────────────────────┤
│ - orderId: string                                   │
│   └─ Required, unique identifier                    │
│   └─ Example: "ORD-20241513-001"                    │
│                                                     │
│ - customerId: string                                │
│   └─ Required, unique customer id                   │
│   └─ Example: "CUST-987"                            │
│                                                     │
│ - total: decimal                                    │
│   └─ Optional, must be > 0                          │
│   └─ Example: 149.90                                │
│                                                     │
│ - description: string                               │
│   └─ Optional, order notes                          │
│   └─ Example: "Sample Electronics Order"            │
│                                                     │
│ - orderDate: DateTime?                              │
│   └─ Optional, ISO 8601 format                      │
│   └─ Example: "2024-01-15T10:30:00Z"               │
└──────────────────────────────────────────────────────┘
		 │
		 │ Validation
		 ▼
┌──────────────────────────────────────────────────────┐
│     OrderValidationResult (Output Model)            │
├──────────────────────────────────────────────────────┤
│ - IsValid: bool                                     │
│   └─ true if all validations pass                   │
│   └─ false if any validation fails                  │
│                                                     │
│ - ErrorMessage: string?                             │
│   └─ null if IsValid = true                         │
│   └─ Error description if IsValid = false           │
│   └─ Example: "Field 'orderId' required..."         │
│                                                     │
│ - ProcessedOrderId: string?                         │
│   └─ null if IsValid = false                        │
│   └─ orderId if IsValid = true                      │
└──────────────────────────────────────────────────────┘
		 │
		 │ HTTP Response (JSON)
		 ▼
┌──────────────────────────────────────────────────────┐
│      HTTP Response JSON (Client Receives)           │
├──────────────────────────────────────────────────────┤
│ Success (HTTP 200):                                 │
│ {                                                   │
│   "status": "success",                              │
│   "message": "Order processed successfully",        │
│   "orderId": "ORD-20241513-001",                   │
│   "customerId": "CUST-987",                         │
│   "total": 149.90,                                  │
│   "processedAt": "2024-01-15T10:30:45.123Z",      │
│   "environment": "Local Development"                │
│ }                                                   │
│                                                     │
│ Error (HTTP 400):                                   │
│ {                                                   │
│   "status": "error",                                │
│   "message": "Field 'customerId' required...",     │
│   "timestamp": "2024-01-15T10:31:00.456Z",        │
│   "environment": "Local Development"                │
│ }                                                   │
└──────────────────────────────────────────────────────┘
```

---

## Deployment Architecture Diagram

### Local Development

```
┌──────────────────────────────────────────────────────┐
│            LOCAL DEVELOPMENT ENVIRONMENT             │
├──────────────────────────────────────────────────────┤
│                                                      │
│  Developer Machine                                  │
│  ┌─────────────────────────────────────┐            │
│  │ Visual Studio / VS Code             │            │
│  │ ├─ OrderFunctionApp project         │            │
│  │ ├─ C# code, models, services       │            │
│  │ └─ Build & Debug                    │            │
│  └──────────┬──────────────────────────┘            │
│             │                                        │
│             ▼                                        │
│  ┌──────────────────────────────────────────┐       │
│  │ Azure Functions Runtime (func start)     │       │
│  │ Local: http://localhost:7071             │       │
│  │ ├─ ProcessOrder endpoint active          │       │
│  │ ├─ Logs to console                        │       │
│  │ └─ No actual Azure resources needed      │       │
│  └────────────┬─────────────────────────────┘       │
│               │                                      │
│               ▼                                      │
│  ┌──────────────────────────────────────────┐       │
│  │ Test Client (PowerShell / cURL)          │       │
│  │ ├─ HTTP POST requests                    │       │
│  │ ├─ JSON payloads                         │       │
│  │ └─ Validate responses                     │       │
│  └──────────────────────────────────────────┘       │
│                                                      │
└──────────────────────────────────────────────────────┘
```

### Azure Cloud Production

```
┌────────────────────────────────────────────────────────────────┐
│              AZURE CLOUD PRODUCTION ENVIRONMENT                 │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌────────────────────────────────────────────────────────┐   │
│  │ Resource Group: order-integration-poc                  │   │
│  │                                                        │   │
│  │  ┌─────────────────────────────────────────────────┐  │   │
│  │  │ Service Bus Namespace                           │  │   │
│  │  │ ├─ Queue: orders-incoming                       │  │   │
│  │  │ ├─ Connection String (Shared Access Key)        │  │   │
│  │  │ └─ Message Retention: 14 days                   │  │   │
│  │  └─────────────────────────────────────────────────┘  │   │
│  │                           │                           │   │
│  │                           ▼                           │   │
│  │  ┌─────────────────────────────────────────────────┐  │   │
│  │  │ Azure Logic App                                 │  │   │
│  │  │ ├─ Trigger: Service Bus queue message           │  │   │
│  │  │ ├─ Actions: Parse, Validate, Call Function      │  │   │
│  │  │ ├─ Error Handling: Scope + runAfter            │  │   │
│  │  │ └─ Logging: Application Insights integration    │  │   │
│  │  └─────────────────────────────────────────────────┘  │   │
│  │                           │                           │   │
│  │                           ▼                           │   │
│  │  ┌─────────────────────────────────────────────────┐  │   │
│  │  │ Azure Function App                              │  │   │
│  │  │ ├─ Runtime: .NET 8 (dotnet-isolated)           │  │   │
│  │  │ ├─ Function: ProcessOrder                       │  │   │
│  │  │ ├─ Trigger: HTTP (Called by Logic App)         │  │   │
│  │  │ ├─ Hosting: App Service Plan (Linux)           │  │   │
│  │  │ ├─ Storage: Storage Account (state)            │  │   │
│  │  │ └─ Auth: Function Code + Optional AAD          │  │   │
│  │  └─────────────────────────────────────────────────┘  │   │
│  │                           │                           │   │
│  │                           ▼                           │   │
│  │  ┌─────────────────────────────────────────────────┐  │   │
│  │  │ Application Insights                            │  │   │
│  │  │ ├─ Telemetry Collection                         │  │   │
│  │  │ ├─ Request Tracking                             │  │   │
│  │  │ ├─ Performance Metrics                          │  │   │
│  │  │ ├─ Custom Logs & Events                         │  │   │
│  │  │ ├─ Failure Analysis                             │  │   │
│  │  │ └─ Dashboards & Alerts                          │  │   │
│  │  └─────────────────────────────────────────────────┘  │   │
│  │                                                        │   │
│  └────────────────────────────────────────────────────────┘   │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

---

## Technology Stack

```
┌─────────────────────────────────────────────────────┐
│         TECHNOLOGY STACK COMPONENTS                  │
├─────────────────────────────────────────────────────┤
│                                                     │
│ Language & Runtime                                  │
│  └─ C# 12 / .NET 8.0                               │
│                                                     │
│ Azure Services                                      │
│  ├─ Azure Service Bus (messaging)                   │
│  ├─ Azure Logic Apps (orchestration)               │
│  ├─ Azure Functions (serverless compute)           │
│  └─ Application Insights (monitoring)              │
│                                                     │
│ SDK & Libraries                                     │
│  ├─ Azure Functions Worker SDK                      │
│  ├─ Azure Service Bus Client SDK                    │
│  ├─ ApplicationInsights SDK                         │
│  ├─ Newtonsoft.Json (JSON handling)                │
│  └─ Microsoft.Extensions (DI, Logging)             │
│                                                     │
│ Development Tools                                   │
│  ├─ Visual Studio 2022+                            │
│  ├─ Azure Functions Core Tools                      │
│  ├─ Azure CLI                                       │
│  ├─ PowerShell 7+                                   │
│  └─ .NET SDK CLI                                    │
│                                                     │
│ Deployment                                          │
│  ├─ ARM Templates (Infrastructure as Code)          │
│  ├─ Azure Container Registry (optional)             │
│  └─ Azure DevOps / GitHub Actions (CI/CD)          │
│                                                     │
│ Monitoring & Observability                          │
│  ├─ Application Insights Logs                       │
│  ├─ KQL (Kusto Query Language)                      │
│  ├─ Custom Metrics & Events                         │
│  └─ Dashboard & Alert Rules                         │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

## Scalability & Performance Characteristics

```
Component              Scalability           Performance Notes
────────────────────────────────────────────────────────────────
Service Bus            Standard/Premium      ~100K messages/day
					   Tier selectable       Partitioning available

Logic Apps             Auto-scaling          ~1000 actions/run
					   Consumption Plan      Triggered every 5s

Azure Functions        Auto-scaling          Code execution < 1s
					   Pay-per-use           Concurrent executions

Application Insights   Auto-scaling          ~500M events/month
					   Ingestion limits      Data retention 30-90d

────────────────────────────────────────────────────────────────
```

---

**Architecture Documentation Version**: 1.0
**Last Updated**: January 15, 2024
