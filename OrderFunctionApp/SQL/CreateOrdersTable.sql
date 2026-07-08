-- SQL Script to create the Orders table in Azure SQL Database
-- Database: OrderIntegrationPOC_DB
-- Author: OrderIntegrationPOC
-- Created: 2024

-- Check if table exists, if not create it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Orders')
BEGIN
	CREATE TABLE Orders (
		Id INT IDENTITY(1,1) PRIMARY KEY,
		OrderId NVARCHAR(50) NOT NULL,
		CustomerId NVARCHAR(50) NOT NULL,
		Total DECIMAL(10,2) NOT NULL,
		Description NVARCHAR(255),
		OrderDate DATETIME NOT NULL,
		CreatedAt DATETIME DEFAULT GETUTCDATE(),
		CONSTRAINT UQ_OrderId UNIQUE (OrderId)
	);

	-- Create indexes for common query patterns
	CREATE INDEX IX_Orders_OrderId ON Orders(OrderId);
	CREATE INDEX IX_Orders_CustomerId ON Orders(CustomerId);
	CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);

	PRINT 'Orders table created successfully with indexes.';
END
ELSE
BEGIN
	PRINT 'Orders table already exists.';
END;

-- Verify table structure
SELECT 
	COLUMN_NAME,
	DATA_TYPE,
	IS_NULLABLE,
	COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Orders'
ORDER BY ORDINAL_POSITION;
