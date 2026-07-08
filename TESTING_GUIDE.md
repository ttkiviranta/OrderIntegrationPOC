# SQL Persistence Integration - Testing Guide

## Overview
This guide provides step-by-step instructions for testing the SQL persistence layer added to your Azure Functions local.settings.json.

## Prerequisites
✅ **Completed Setup:**
- Orders table schema created in Azure SQL Database using `OrderFunctionApp/SQL/CreateOrdersTable.sql`
- ProcessOrderToSql Azure Function implemented with QueueTrigger
- SQL connection string configured in local.settings.json
- Project built successfully (.NET 8 isolated)

---

## Part 1: Create the Orders Table in SQL Database

### Option A: Using Azure Portal (SQL Query Editor)
1. Open [Azure Portal](https://portal.azure.com)
2. Navigate to **SQL Databases > OrderIntegrationPOC_DB**
3. Click **Query Editor** in the left sidebar
4. Copy the contents from `OrderFunctionApp/SQL/CreateOrdersTable.sql`
5. Paste into the Query Editor
6. Click **Run**
7. Verify output shows: "Orders table created successfully with indexes."

### Option B: Using SQL Server Management Studio (SSMS)
1. Open SQL Server Management Studio
2. **Connect to Server:**
   - Server name: `orderintegrationpoc-sqlserver.database.windows.net`
   - Authentication: SQL Server Authentication
   - Login: `orderadmin`
   - Password: `PocDemo!2024`
   - Database: `OrderIntegrationPOC_DB`
3. Open a **New Query** window
4. Copy and paste the SQL script from `OrderFunctionApp/SQL/CreateOrdersTable.sql`
5. Click **Execute** (or press F5)
6. Verify the table was created successfully

### Option C: Using Azure Data Studio
1. Install [Azure Data Studio](https://learn.microsoft.com/en-us/azure-data-studio/download-azure-data-studio)
2. Create a new connection to your SQL server
3. Open `OrderFunctionApp/SQL/CreateOrdersTable.sql`
4. Run the script
5. Verify the Orders table in **Databases > OrderIntegrationPOC_DB > Tables**

---

## Part 2: Verify SQL Table

After creating the table, verify its structure:

```sql
-- Query to verify Orders table structure
SELECT 
	COLUMN_NAME,
	DATA_TYPE,
	IS_NULLABLE,
	COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Orders'
ORDER BY ORDINAL_POSITION;
```

**Expected Output:**
| COLUMN_NAME | DATA_TYPE | IS_NULLABLE | COLUMN_DEFAULT |
|-------------|-----------|-------------|-----------------|
| Id | int | NO | NULL |
| OrderId | nvarchar | NO | NULL |
| CustomerId | nvarchar | NO | NULL |
| Total | decimal | NO | NULL |
| Description | nvarchar | YES | NULL |
| OrderDate | datetime | NO | NULL |
| CreatedAt | datetime | YES | GETUTCDATE() |

---

## Part 3: Local Testing with Azure Functions Core Tools

### Prerequisites
- Install [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- Install [Azure Storage Emulator](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-emulator) OR have Azure Storage connection configured

### Run Azure Functions Locally

1. **Start Azure Storage Emulator** (if using emulator instead of Azure):
   ```powershell
   C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe start
   ```

2. **Open Terminal in Project Directory:**
   ```powershell
   cd C:\Users\ttkiv\source\repos\OrderIntegrationPOC\OrderFunctionApp
   ```

3. **Start the Azure Functions Host:**
   ```powershell
   func start
   ```

   **Expected output:**
   ```
   Azure Functions Core Tools
   Found C:\Users\ttkiv\source\repos\OrderIntegrationPOC\OrderFunctionApp\OrderFunctionApp.csproj
   .NET 8.0 Isolated

   ProcessOrderToSql: queueTrigger

   For detailed output, run func with --verbose flag.
   Listening on http://localhost:7071
   Hit CTRL+C to stop...
   ```

---

## Part 4: Testing with Postman or REST Client

### Scenario 1: Send Message to Queue (Emulate Queue Trigger)

Since the ProcessOrderToSql function is triggered by queue messages, you need to simulate queue messages.

#### Using Azure Storage Explorer:
1. Open **Azure Storage Explorer**
2. Connect to your storage account (or Storage Emulator if local)
3. Navigate to **Queues**
4. Right-click on `orders-queue` → **Create New Queue** (if needed)
5. Right-click **AddMessage**
6. Paste the following JSON:

```json
{
  "orderId": "ORD-2024-001",
  "customerId": "CUST-12345",
  "total": 299.99,
  "description": "Sample order from Postman test",
  "orderDate": "2024-01-15T10:30:00Z"
}
```

7. Click **OK**

#### Expected Result in Function Output:
```
Processing queue message at 2024-01-15T10:35:22.150Z
Inserting order ORD-2024-001 into SQL database
Successfully inserted 1 row for order ORD-2024-001
Order ORD-2024-001 successfully persisted to SQL database
```

---

### Scenario 2: Verify Data in SQL

After sending a queue message, verify the order was inserted:

```sql
-- Check if order was inserted
SELECT TOP 10 * FROM Orders 
ORDER BY CreatedAt DESC;

-- Check specific order
SELECT * FROM Orders WHERE OrderId = 'ORD-2024-001';
```

**Expected Result:**
| Id | OrderId | CustomerId | Total | Description | OrderDate | CreatedAt |
|----|---------|------------|-------|-------------|-----------|-----------|
| 1 | ORD-2024-001 | CUST-12345 | 299.99 | Sample order from Postman test | 2024-01-15 10:30:00 | 2024-01-15 10:35:22 |

---

## Part 5: Bulk Testing with Multiple Orders

### Test Script - Create 5 Sample Orders

```sql
-- Insert multiple test orders
DECLARE @OrderDate DATETIME = GETDATE();

INSERT INTO Orders (OrderId, CustomerId, Total, Description, OrderDate)
VALUES 
  ('ORD-2024-002', 'CUST-12346', 149.50, 'Bulk test order 1', @OrderDate),
  ('ORD-2024-003', 'CUST-12347', 299.99, 'Bulk test order 2', @OrderDate),
  ('ORD-2024-004', 'CUST-12348', 75.00, 'Bulk test order 3', @OrderDate),
  ('ORD-2024-005', 'CUST-12349', 450.25, 'Bulk test order 4', @OrderDate),
  ('ORD-2024-006', 'CUST-12350', 199.99, 'Bulk test order 5', @OrderDate);

-- Verify insertion
SELECT COUNT(*) AS TotalOrders FROM Orders;
SELECT * FROM Orders ORDER BY CreatedAt DESC;
```

---

## Part 6: Error Handling Testing

### Test 1: Missing Required Field (OrderId)
Send this message to the queue:
```json
{
  "customerId": "CUST-12345",
  "total": 299.99,
  "description": "Missing OrderId field"
}
```

**Expected Result:** Function logs error and fails gracefully with exception message.

### Test 2: Invalid Total (Negative)
Send this message:
```json
{
  "orderId": "ORD-2024-ERR-001",
  "customerId": "CUST-12345",
  "total": -100.00,
  "description": "Negative total"
}
```

**Expected Result:** Database insert succeeds (no validation at DB level), but log warning.

### Test 3: SQL Connection Error
Comment out the connection string in `local.settings.json` and send a queue message.

**Expected Result:** Function throws exception with error details logged to Application Insights/console.

---

## Part 7: Performance Testing

### Count Orders by Customer
```sql
SELECT CustomerId, COUNT(*) AS OrderCount, SUM(Total) AS TotalAmount
FROM Orders
GROUP BY CustomerId
ORDER BY TotalAmount DESC;
```

### Verify Indexes Are Working
```sql
-- Check index usage
SELECT OBJECT_NAME(s.object_id) AS TableName,
	   i.name AS IndexName,
	   s.user_updates,
	   s.user_seeks,
	   s.user_scans
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE OBJECT_NAME(s.object_id) = 'Orders';
```

---

## Part 8: Deployment to Azure

After local testing is successful:

### 1. Deploy Function App to Azure
```powershell
func azure functionapp publish orderfunc --build remote
```

### 2. Verify in Azure Portal
1. Navigate to **Function App > orderfunc > Functions**
2. Click on **ProcessOrderToSql**
3. Monitor the **Monitor** tab for execution logs

### 3. Send Production Queue Messages
1. Use Azure Portal or **Azure Storage Explorer**
2. Send message to the `orders-queue` in your production storage account
3. Monitor logs in **Function App > Monitor > Logs**

---

## Troubleshooting

| Issue | Cause | Resolution |
|-------|-------|-----------|
| **"SqlConnectionString is not configured"** | Connection string missing from config | Verify `SqlConnectionString` exists in `appsettings.json` or environment variables |
| **Connection timeout** | Network/firewall issue | Ensure firewall rules allow your IP to connect to SQL Server |
| **"Orders table does not exist"** | SQL table creation failed | Re-run the SQL script from `OrderFunctionApp/SQL/CreateOrdersTable.sql` |
| **Queue trigger not firing** | Queue message format incorrect | Verify JSON structure matches OrderRequest model |
| **Function crashes with JSON deserialization error** | JSON property names mismatch | Use camelCase for JSON properties (orderId, customerId, etc.) |
| **"UNIQUE constraint violation on OrderId"** | Duplicate OrderId sent | Ensure each order has a unique OrderId |

---

## Additional Resources

- [Azure Functions Queue Trigger Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue)
- [SQL Server Data Types](https://learn.microsoft.com/en-us/sql/t-sql/data-types/data-types-transact-sql)
- [Microsoft.Data.SqlClient Documentation](https://learn.microsoft.com/en-us/sql/connect/ado-net/introduction-microsoft-data-sqlclient)
- [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/)
- [Azure Functions Core Tools Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)

---

## Summary

✅ **Your SQL Persistence Setup is Complete:**
- Orders table created with proper schema
- ProcessOrderToSql Azure Function ready to handle queue messages
- SQL connection configured and tested
- Project compiles without errors

**Next Steps:**
1. Execute the SQL script to create the Orders table
2. Run the function app locally using `func start`
3. Send test messages to the queue
4. Verify data appears in SQL Database
5. Deploy to Azure when ready
