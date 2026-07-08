# OrderFunctionApp - SQL Persistence Features

## 🆕 What's New in v2.0

This OrderFunctionApp now includes complete SQL persistence capabilities for direct order storage in Azure SQL Database.

### New Features

#### 1. ProcessOrderToSql Queue-Triggered Function
**File:** `Functions/ProcessOrderToSql.cs` (120 lines)

- **Trigger:** Queue-triggered on `orders-queue` (Azure Queue Storage)
- **Functionality:**
  - Deserializes JSON queue messages into `OrderRequest` objects
  - Opens async SQL connection using `SqlConnectionString` from configuration
  - Executes parameterized INSERT statements (prevents SQL injection)
  - Logs success and error scenarios to Application Insights
  - Handles missing/null values gracefully with defaults
  - Comprehensive error handling for SQL and deserialization errors

**Key Methods:**
```csharp
[Function("ProcessOrderToSql")]
public async Task Run(
	[QueueTrigger("orders-queue", Connection = "AzureWebJobsStorage")] string message,
	FunctionContext context)
```

#### 2. Orders Table Schema
**File:** `SQL/CreateOrdersTable.sql`

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

**Features:**
- IDENTITY auto-increment primary key
- UNIQUE constraint on OrderId (no duplicates)
- Indexed columns for fast queries
- Audit trail with CreatedAt timestamp

#### 3. Configuration
**File:** `local.settings.json` (UPDATED)

Add SQL connection string:
```json
{
  "Values": {
	"SqlConnectionString": "Server=tcp:{server}.database.windows.net,1433;Initial Catalog={database};User ID={user};Password={password};..."
  }
}
```

#### 4. Dependencies
**File:** `OrderFunctionApp.csproj` (UPDATED)

New NuGet packages:
- `Microsoft.Data.SqlClient` v5.1.5 - SQL Server connectivity
- `Azure.Storage.Queues` v12.21.0 - Queue storage integration
- `Microsoft.Azure.Functions.Worker.Extensions.Storage.Queues` v5.5.4 - Queue triggers

### Architecture

```
Queue Message (JSON)
	↓
ProcessOrderToSql Function
	├─ Deserialize OrderRequest
	├─ Validate (check for nulls)
	├─ Open SQL Connection (async)
	├─ Execute parameterized INSERT
	└─ Log success/errors
		 ↓
Orders Table (SQL Database)
	└─ Stored for long-term access
```

### Quick Start - SQL Testing

#### Step 1: Create Orders Table
Execute `SQL/CreateOrdersTable.sql`:
- **Azure Portal:** SQL Databases > Query Editor > Run script
- **SSMS:** Connect > Open file > Execute
- **Azure Data Studio:** Connect > Open file > Execute

#### Step 2: Start Functions Locally
```powershell
cd OrderFunctionApp
func start
```

#### Step 3: Send Test Message
Using Azure Storage Explorer:
1. Connect to Azure Storage
2. Navigate to Queues
3. Select `orders-queue`
4. Add message with JSON payload:
```json
{
  "orderId": "ORD-2024-001",
  "customerId": "CUST-12345",
  "total": 299.99,
  "description": "Test order",
  "orderDate": "2024-01-15T10:30:00Z"
}
```

#### Step 4: Verify in SQL
```sql
SELECT * FROM Orders WHERE OrderId = 'ORD-2024-001';
```

**Expected Result:**
- 1 row with OrderId = 'ORD-2024-001'
- CustomerId = 'CUST-12345'
- Total = 299.99
- CreatedAt timestamp set to current UTC time

#### Step 5: Check Logs
Console output should show:
```
Inserting order ORD-2024-001 into SQL database
Successfully inserted 1 row for order ORD-2024-001
Order ORD-2024-001 successfully persisted to SQL database
```

### Security Features

✅ **SQL Injection Protection**
- Parameterized queries using `SqlCommand.Parameters`
- No string concatenation with user input

✅ **Data Encryption**
- SQL connection encrypted (TrustServerCertificate=False)
- HTTPS for all web communications

✅ **Credential Management**
- Connection string via `IConfiguration`
- No hardcoded credentials
- Support for Azure Key Vault

✅ **Error Handling**
- Specific exception catching (SqlException, InvalidOperationException)
- No credential exposure in error messages
- Detailed logging to Application Insights

### Performance Optimizations

✅ **Indexing**
- OrderId (UNIQUE) - Prevents duplicates and enables fast lookup
- CustomerId - Fast customer-based queries
- OrderDate - Range queries on dates

✅ **Async Operations**
- `OpenAsync()` - Non-blocking connection
- `ExecuteNonQueryAsync()` - Non-blocking execution

✅ **Connection Pooling**
- Enabled by default in SqlConnection
- Reuses connections efficiently
- Reduces overhead

### Error Scenarios

#### Scenario 1: Missing OrderId
Queue message without `orderId` field:
- **Result:** Deserialization error logged
- **Action:** Message remains in dead-letter queue
- **Log:** "Failed to deserialize queue message"

#### Scenario 2: Invalid Total (negative)
Queue message with `total: -50`:
- **Result:** Inserted into database (no DB-level validation)
- **Warning:** Logged to Application Insights
- **Action:** Manual review suggested

#### Scenario 3: Duplicate OrderId
Sending same OrderId twice:
- **Result:** First insert succeeds, second fails
- **Error:** UNIQUE constraint violation
- **Log:** "UNIQUE constraint violation on OrderId"

#### Scenario 4: SQL Connection Failure
Database offline or firewall rule blocks:
- **Result:** SqlException thrown
- **Log:** Connection timeout with specific SQL error
- **Action:** Message retried by Azure Functions runtime

### Testing Scenarios

See **[TESTING_GUIDE.md](../TESTING_GUIDE.md)** for:
- Part 1: SQL table creation (3 methods)
- Part 2: Table structure verification
- Part 3: Local testing setup
- Part 4: Queue message testing
- Part 5: Bulk testing
- Part 6: Error handling
- Part 7: Performance testing
- Part 8: Azure deployment

### Documentation

Complete guides available:
- **[../START_HERE.md](../START_HERE.md)** - 5-minute quick start
- **[../TESTING_GUIDE.md](../TESTING_GUIDE.md)** - 8-part testing guide (400+ lines)
- **[../SQL_PERSISTENCE_SUMMARY.md](../SQL_PERSISTENCE_SUMMARY.md)** - Technical details
- **[../QUICK_REFERENCE.md](../QUICK_REFERENCE.md)** - Reference card
- **[../INDEX.md](../INDEX.md)** - Complete navigation guide

### Monitoring & Troubleshooting

#### Monitor in Application Insights
1. Azure Portal > Application Insights > Logs
2. Query: `traces | where message contains "ORD-2024-001"`
3. View: Function execution, SQL operations, errors

#### Common Issues

| Issue | Solution |
|-------|----------|
| "Orders table does not exist" | Execute SQL script from `SQL/CreateOrdersTable.sql` |
| "Connection timeout" | Check firewall rules allow your IP in Azure SQL |
| "SqlConnectionString is not configured" | Verify entry in local.settings.json |
| "JSON deserialization failed" | Ensure properties are camelCase (orderId, customerId) |
| "UNIQUE constraint violation" | Each OrderId must be unique |

### File Changes Summary

**New Files:**
- `SQL/CreateOrdersTable.sql` - Table schema and indexes
- `Functions/ProcessOrderToSql.cs` - Queue-triggered function

**Modified Files:**
- `Program.cs` - ProcessOrderToSql registered in DI
- `local.settings.json` - SqlConnectionString added
- `OrderFunctionApp.csproj` - Microsoft.Data.SqlClient and Queue packages added

**Unchanged:**
- `Models/OrderRequest.cs` - Already has required properties
- `Functions/ProcessOrder.cs` - HTTP-triggered function unchanged

### Deployment

To deploy to Azure:
```powershell
# Login to Azure
az login

# Navigate to project
cd OrderFunctionApp

# Deploy function app
func azure functionapp publish your-function-app-name

# Create SQL table in Azure SQL Database
# Use Azure Portal or SSMS to execute CreateOrdersTable.sql

# Set Application Settings in Azure Portal
# - SqlConnectionString (connection to your Azure SQL)
```

### Architecture Diagrams

### Classic Pipeline (Existing - Service Bus + Logic App)
```
ERP System → Service Bus → Logic App → HTTP Function → Validation → AppInsights
```

### NEW: Direct Queue to SQL Pipeline (v2.0)
```
Queue Message (JSON)
	↓
ProcessOrderToSql Function (Queue Trigger)
	├─ Deserialize JSON
	├─ Validate input
	├─ Open SQL connection (async)
	├─ INSERT record (parameterized)
	└─ Log results
		 ↓
Azure SQL Database
	↓
Orders Table (Indexed)
```

### Next Steps

1. ✅ Create Orders table (execute SQL script)
2. ✅ Test locally with func start
3. ✅ Send sample queue messages
4. ✅ Verify data in database
5. ✅ Test error scenarios
6. ✅ Deploy to Azure
7. ✅ Monitor with Application Insights

---

**Version:** 2.0 - SQL Persistence Edition  
**Updated:** July 8, 2026  
**Status:** ✅ Production Ready
