# Entity Framework Core Integration - Testing Guide

## Overview
The OrderIntegrationPOC Azure Functions project has been successfully upgraded with Entity Framework Core. The `ProcessOrderToSql` function now uses EF Core for database persistence instead of ADO.NET.

## What Changed
- ✅ Added Entity Framework Core NuGet packages
- ✅ Created `Order` entity class with full properties
- ✅ Created `OrderIntegrationContext` DbContext with SQL Server configuration
- ✅ Registered `DbContextFactory<OrderIntegrationContext>` for dependency injection
- ✅ Refactored `ProcessOrderToSql` to inject and use EF Core
- ✅ Generated and applied migration `InitialCreate` to Azure SQL database
- ✅ Orders table created with proper constraints and indexes

## Database Schema
The Orders table in `OrderIntegrationPOC_DB` has been created with the following structure:

```sql
CREATE TABLE [Orders] (
	[Id] int NOT NULL IDENTITY,
	[OrderId] nvarchar(50) NOT NULL,
	[CustomerId] nvarchar(50) NOT NULL,
	[Total] decimal(10, 2) NOT NULL,
	[Description] nvarchar(255) NULL,
	[OrderDate] datetime2 NOT NULL,
	[CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
	CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
	CONSTRAINT [UQ_Orders_OrderId] UNIQUE ([OrderId])
);

-- Indexes
CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);
CREATE INDEX [IX_Orders_OrderDate] ON [Orders] ([OrderDate]);
```

## Testing the EF Core Integration

### Prerequisites
- Azure Storage Emulator running (for Queue Storage)
- Local SQL Server or Azure SQL database connection configured in `local.settings.json`
- Visual Studio or VS Code with Azure Functions tools installed

### Step 1: Verify Local Settings
Ensure `OrderFunctionApp/local.settings.json` contains valid settings:

```json
{
  "Values": {
	"AzureWebJobsStorage": "UseDevelopmentStorage=true",
	"FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
	"SqlConnectionString": "Server=tcp:orderintegrationpoc-sqlserver.database.windows.net,1433;Initial Catalog=OrderIntegrationPOC_DB;...",
	"ServiceBusConnection": "...",
	"ServiceBusQueueName": "orders-incoming"
  }
}
```

### Step 2: Start the Functions Locally
From the project directory, run:

```bash
func start
```

You should see output similar to:
```
OrderIntegrationPOC$ func start
Found C:\Users\ttkiv\source\repos\OrderIntegrationPOC\OrderFunctionApp\local.settings.json
Azure Functions Core Tools
Worker runtime doesn't exist
Install worker runtime
Installing worker runtime from /.../dotnet-isolated...
...
Workers initialized in 2.345s
Host started (PID: 12345)

ProcessOrderToSql: queueTrigger
	Queue name: orders-queue
	Connection name: AzureWebJobsStorage
```

### Step 3: Send a Test Message to the Queue

#### Using Azure CLI
```bash
az storage message put --queue-name orders-queue \
  --content '{"orderId":"ORD-2024-001","customerId":"CUST-001","total":299.99,"description":"Test order from EF Core integration","orderDate":"2024-01-15T10:30:00Z"}' \
  --account-name <your-storage-account>
```

#### Using Azure Storage Explorer
1. Open Azure Storage Explorer
2. Navigate to your storage account → Queues → `orders-queue`
3. Click "Add Message"
4. Paste the following JSON:
```json
{
  "orderId": "ORD-2024-001",
  "customerId": "CUST-001",
  "total": 299.99,
  "description": "Test order from EF Core integration",
  "orderDate": "2024-01-15T10:30:00Z"
}
```
5. Click "OK"

#### Using Code
Create a simple .NET console app or PowerShell script:
```powershell
$connectionString = "UseDevelopmentStorage=true"
$queueName = "orders-queue"
$message = @{
	orderId = "ORD-EF-001"
	customerId = "CUST-EF-001"
	total = 149.50
	description = "EF Core test order"
	orderDate = [DateTime]::UtcNow.ToString("O")
} | ConvertTo-Json

# Using az CLI
az storage message put --queue-name $queueName --content $message --connection-string $connectionString
```

### Step 4: Monitor Function Execution
The Function output should show:
```
ProcessOrderToSql: Function started (Id=xyz-123)
Processing queue message at 2024-07-08T10:30:45.1234567Z
Inserting order ORD-2024-001 into database using Entity Framework Core
Successfully inserted 1 row(s) for order ORD-2024-001
Order ORD-2024-001 successfully persisted to SQL database
ProcessOrderToSql: Function completed (Success, Id=xyz-123, Duration=523ms)
```

### Step 5: Verify Data in Database
Connect to the Azure SQL database and verify the order was inserted:

```sql
-- Query the Orders table
SELECT * FROM Orders 
WHERE OrderId IN ('ORD-2024-001', 'ORD-EF-001')
ORDER BY CreatedAt DESC;

-- Check row count
SELECT COUNT(*) as TotalOrders FROM Orders;

-- View recent orders
SELECT TOP 10 
	Id, 
	OrderId, 
	CustomerId, 
	Total, 
	Description, 
	OrderDate, 
	CreatedAt
FROM Orders
ORDER BY CreatedAt DESC;
```

## Testing Scenarios

### Scenario 1: Valid Order Insertion
**Input Message:**
```json
{
  "orderId": "ORD-TEST-001",
  "customerId": "CUST-TEST-001",
  "total": 250.00,
  "description": "Test order - valid data",
  "orderDate": "2024-07-08T10:30:00Z"
}
```

**Expected Result:**
- ✅ Function executes successfully
- ✅ Log shows "Successfully inserted 1 row(s)"
- ✅ Order appears in database with CreatedAt timestamp

### Scenario 2: Order with Minimal Data
**Input Message:**
```json
{
  "orderId": "ORD-TEST-002",
  "customerId": "CUST-TEST-002",
  "total": 100.50
}
```

**Expected Result:**
- ✅ Function executes successfully
- ✅ Description is NULL
- ✅ OrderDate defaults to current UTC time
- ✅ CreatedAt is set automatically

### Scenario 3: Duplicate OrderId (UNIQUE Constraint)
**Input Message (same OrderId as Scenario 1):**
```json
{
  "orderId": "ORD-TEST-001",
  "customerId": "CUST-DUPLICATE",
  "total": 300.00
}
```

**Expected Result:**
- ❌ Function fails with DbUpdateException
- ❌ Log shows "Database error occurred"
- ❌ Duplicate key constraint violation in database error
- ✅ Error is properly logged and exception is thrown

### Scenario 4: Invalid JSON in Queue Message
**Input Message:**
```
{ invalid json
```

**Expected Result:**
- ❌ Function fails with JSON deserialization error
- ❌ Log shows "Failed to deserialize queue message"
- ✅ Error is properly caught and logged

## Monitoring and Diagnostics

### Application Insights
If Application Insights is configured, you can query telemetry:

```kusto
// Function invocation count
customMetrics
| where name == "ProcessOrderToSql"
| summarize Count=sum(value) by bin(timestamp, 1h)

// Trace logs
traces
| where customDimensions.["prop__{OriginalFormat}"] contains "Order"
| project timestamp, message, severityLevel
| order by timestamp desc

// Failed operations
traces
| where severityLevel >= 2  // Error or higher
| where customDimensions.["Category"] == "ProcessOrderToSql"
| project timestamp, message, severityLevel
```

### Local Diagnostics
View the full function logs in the local terminal where you ran `func start`:
```
ProcessOrderToSql: Function started
Processing queue message at [timestamp]
...detailed debug output...
Order [OrderId] successfully persisted to SQL database
ProcessOrderToSql: Function completed
```

## Key EF Core Features Used

1. **DbContextFactory Pattern**: Ensures proper DbContext lifecycle for Azure Functions (each invocation gets a new context)
2. **Retry Policy**: Configured with 3 retry attempts and 10-second max delay
3. **Constraints and Indexes**:
   - UNIQUE constraint on OrderId prevents duplicate orders
   - Indexes on CustomerId and OrderDate optimize queries
4. **SQL Default Values**: CreatedAt timestamp is set by SQL Server (GETUTCDATE())
5. **Exception Handling**: Comprehensive error handling for DbUpdateException and other EF operations

## Rollback Instructions

If you need to revert to ADO.NET implementation:

1. Revert the migration:
```bash
$env:SqlConnectionString = "..."
cd OrderFunctionApp
dotnet ef database update 0
```

2. Git revert changes:
```bash
git revert HEAD~1
```

## Next Steps

1. ✅ **Completed**: EF Core integration
2. ✅ **Completed**: Migration created and applied
3. 🔄 **Test**: Run the function with sample queue messages
4. 🔄 **Monitor**: Watch Application Insights for performance metrics
5. 📈 **Optimize**: Add additional indexes if needed based on query patterns
6. 🚀 **Deploy**: Deploy to Azure Functions with the EF Core implementation

## Troubleshooting

### "SqlConnectionString is not configured"
- Ensure `local.settings.json` contains valid SqlConnectionString
- Check that the connection string format is correct
- Verify firewall rules allow your IP address to connect to Azure SQL

### "Connection timeout"
- Verify network connectivity to Azure SQL server
- Check firewall rules on Azure SQL
- Verify connection string has correct server name and port (1433)
- Ensure "Connection Timeout=30" is set in the connection string

### "UNIQUE constraint violation on OrderId"
- This is expected if you test with the same OrderId twice
- Change the OrderId in test messages to make them unique
- Or clear the Orders table for testing: `TRUNCATE TABLE Orders`

### Function doesn't appear to process messages
- Check that Azure Storage Emulator is running
- Verify queue name matches: `orders-queue`
- Check AzureWebJobsStorage connection string in local.settings.json
- Look for errors in the function log output

## Support and Additional Resources

- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [EF Core SQL Server Provider](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/)
- [Azure Functions with EF Core](https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library)
- [DbContextFactory](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#using-a-dbcontext-factory-eg-for-blazor)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
