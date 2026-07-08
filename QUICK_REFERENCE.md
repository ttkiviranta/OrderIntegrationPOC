# Quick Reference - SQL Persistence Integration

## 📌 Files Created/Modified

| File | Action | Purpose |
|------|--------|---------|
| `OrderFunctionApp/SQL/CreateOrdersTable.sql` | ✨ **CREATED** | SQL script to create Orders table |
| `OrderFunctionApp/OrderFunctionApp.csproj` | 🔧 **FIXED** | Updated NuGet package versions |
| `TESTING_GUIDE.md` | ✨ **CREATED** | Comprehensive testing documentation |
| `SQL_PERSISTENCE_SUMMARY.md` | ✨ **CREATED** | Implementation summary |

## 🎯 Immediate Actions

### 1️⃣ Create Orders Table (Choose One Method)

**Azure Portal:**
```
portal.azure.com → SQL Databases → OrderIntegrationPOC_DB → Query Editor
Copy/paste content from: OrderFunctionApp/SQL/CreateOrdersTable.sql
```

**SSMS:**
```sql
-- Connect to: orderintegrationpoc-sqlserver.database.windows.net
-- Database: OrderIntegrationPOC_DB
-- Open: OrderFunctionApp/SQL/CreateOrdersTable.sql
-- Press: F5
```

### 2️⃣ Test Locally

```powershell
# Start Azure Storage Emulator
C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe start

# Start Functions
cd C:\Users\ttkiv\source\repos\OrderIntegrationPOC\OrderFunctionApp
func start
```

### 3️⃣ Send Test Queue Message

Using **Azure Storage Explorer:**
```json
{
  "orderId": "ORD-2024-001",
  "customerId": "CUST-12345",
  "total": 299.99,
  "description": "Test order",
  "orderDate": "2024-01-15T10:30:00Z"
}
```
Send to: `orders-queue`

### 4️⃣ Verify in SQL

```sql
SELECT * FROM Orders WHERE OrderId = 'ORD-2024-001';
```

---

## 🔗 Key Connections

```
Queue Message (JSON)
	↓
[ProcessOrderToSql] Azure Function
	↓
local.settings.json → SqlConnectionString
	↓
Azure SQL Database → OrderIntegrationPOC_DB
	↓
[Orders] Table (newly created)
```

---

## 📊 Orders Table Schema

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

-- Indexes for performance
CREATE INDEX IX_Orders_OrderId ON Orders(OrderId);
CREATE INDEX IX_Orders_CustomerId ON Orders(CustomerId);
CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);
```

---

## 🧪 Testing Scenarios

| Scenario | Queue Message | Expected Result |
|----------|---------------|-----------------|
| **Happy Path** | Valid order JSON | ✅ Inserted into Orders table |
| **Missing OrderId** | No orderId field | ❌ Deserialization error logged |
| **Invalid Total** | Negative value | ✅ Inserted (no DB validation) |
| **Duplicate OrderId** | OrderId already exists | ❌ UNIQUE constraint violation |
| **Null Description** | description: null | ✅ Stored as empty string |
| **Missing OrderDate** | No orderDate field | ✅ Uses DateTime.UtcNow |

---

## ⚙️ Configuration

### local.settings.json
```json
{
  "SqlConnectionString": "Server=tcp:orderintegrationpoc-sqlserver.database.windows.net,1433;Initial Catalog=OrderIntegrationPOC_DB;..."
}
```

### Connection Details
- **Server:** orderintegrationpoc-sqlserver.database.windows.net
- **Database:** OrderIntegrationPOC_DB
- **User:** orderadmin
- **Port:** 1433
- **Encryption:** Yes
- **Timeout:** 30 seconds

---

## 🚨 Troubleshooting

| Error | Fix |
|-------|-----|
| "Orders table does not exist" | Run SQL script from CreateOrdersTable.sql |
| "SqlConnectionString is not configured" | Verify entry in local.settings.json |
| "Connection timeout" | Check firewall rules allow your IP |
| "JSON deserialization failed" | Ensure properties are camelCase (orderId, customerId) |
| "UNIQUE constraint violation" | Each OrderId must be unique |
| Queue trigger not firing | Verify connection string AzureWebJobsStorage points to valid storage |

---

## 📈 Performance Queries

```sql
-- Count total orders
SELECT COUNT(*) AS TotalOrders FROM Orders;

-- Orders by customer
SELECT CustomerId, COUNT(*) AS Count, SUM(Total) AS Total
FROM Orders
GROUP BY CustomerId;

-- Orders by date range
SELECT * FROM Orders 
WHERE OrderDate > '2024-01-01' 
ORDER BY OrderDate DESC;
```

---

## 🔐 Security Notes

✅ **Implemented:**
- Parameterized SQL queries (prevent SQL injection)
- SQL connection encryption (TrustServerCertificate=False)
- Secure authentication (SQL Server user)
- Connection pooling (reuse connections)

⚠️ **Reminders:**
- Don't commit local.settings.json with credentials to source control
- Use Azure Key Vault for production secrets
- Use Managed Identity for Azure-to-Azure communication
- Never log sensitive data

---

## 📞 Support Resources

- **Testing Guide:** `TESTING_GUIDE.md`
- **Summary:** `SQL_PERSISTENCE_SUMMARY.md`
- **SQL Script:** `OrderFunctionApp/SQL/CreateOrdersTable.sql`
- **Function Code:** `OrderFunctionApp/Functions/ProcessOrderToSql.cs`
- **Model:** `OrderFunctionApp/Models/OrderRequest.cs`

---

## ✅ Checklist

- [ ] Execute SQL script to create Orders table
- [ ] Verify table exists with correct schema
- [ ] Start Azure Functions locally (`func start`)
- [ ] Send test message to orders-queue
- [ ] Verify order appears in SQL database
- [ ] Check logs in console for success message
- [ ] Test error scenarios (missing fields, duplicates)
- [ ] Deploy to Azure when satisfied with local testing

---

**Status:** ✅ Ready to execute
