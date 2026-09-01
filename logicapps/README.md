order-notification Logic App

Overview

This folder contains a Logic Apps workflow definition (order-notification.json) that triggers when a new row is inserted into the Orders table and sends notifications via both Office 365 email and Microsoft Teams. The Logic App is intended to be used with the existing OrderIntegrationPOC architecture: Azure Functions produce and persist Orders into the SQL database; this Logic App watches the Orders table and notifies stakeholders when new orders arrive.

Files

- order-notification.json  - Logic App workflow definition (template)
- README.md                - This file

Requirements

- Azure subscription and permissions to create Logic Apps and API connections
- SQL Server instance accessible by Logic Apps with a configured connection (same DB used by EF Core)
- Office 365 account (to send email) and/or Microsoft Teams connector configured

High-level steps

1. Create connections
   - In the Azure Portal, create an API connection for SQL Server (SQL connector). During creation provide server name, database name, authentication details (SQL auth or Managed Identity).
   - Create an API connection for Office 365 Outlook (to send email) and an API connection for Microsoft Teams (to post messages).

2. Create a Logic App
   - In the Azure Portal, create a new Logic App (Consumption or Standard plan as required).
   - Open the Logic App Designer and choose "Import" or "Template" and upload the file logicapps/order-notification.json.

3. Configure template parameters
   - SqlConnection: set to the resource id of the SQL API connection created earlier (example value: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Web/connections/sql)
   - Office365Connection: set to the resource id of the Office 365 Outlook API connection
   - TeamsConnection: set to the resource id of the Microsoft Teams API connection
   - RecipientEmail: set to the recipient email address for the email action
   - TeamId and ChannelId: set to the target Teams team and channel identifiers where messages will be posted
   - TableName: default is dbo.Orders (change only if your table name differs)
   - PollingInterval: default PT1M (1 minute). Do not set lower than your expected throughput to avoid throttling.

4. Save and test
   - Save the Logic App and ensure it is enabled.
   - Insert a test order into the Orders table (or let the existing Function pipeline insert a real test order). The Logic App should trigger within one minute and send notifications to both email and Teams.

5. Verify
   - Check Logic App run history to confirm trigger details and payload values.
   - Check recipient inbox and Teams channel for the notification messages.

Notes and recommendations

- The workflow template uses the SQL "When an item is created" trigger. That trigger polls the database at the configured polling interval (1 minute). Polling frequency affects cost and performance; choose a sensible interval for your environment.
- Use Managed Identity when possible to avoid embedding credentials. For Logic Apps Standard you can use system-assigned identity and grant the SQL user DATA READER or execute permissions as needed.
- The template sends an Office 365 email and a Microsoft Teams Adaptive Card with rich formatting. You can customize the Adaptive Card JSON to change layout, styling, or add actions as needed.
- This Logic App reads from the same Orders table that the ProcessOrderToSql Function writes to. The Logic App is read-only and does not modify the database.
- For high-volume systems consider using event-driven architectures (Service Bus / Event Grid) rather than polling.

Example notification body (text)

Order created:
- OrderId: @{triggerBody()?['OrderId']}
- CustomerId: @{triggerBody()?['CustomerId']}
- Total: @{triggerBody()?['Total']}
- CreatedAt: @{triggerBody()?['CreatedAt']}

Security

- Do not commit production credentials. The JSON template contains placeholders for connection resource ids and recipient email addresses.
- Use role-based access control (RBAC) and least privilege for the API connections.

How this fits into the OrderIntegrationPOC pipeline

1. Client or Service posts an order to the HTTP endpoint or enqueues a message to orders-queue.
2. Queue-triggered Function ProcessOrderToSql persists the order to the Orders table in SQL Server.
3. Logic App polls the Orders table and triggers when a new record appears.
4. Logic App sends notifications (email and Teams) to stakeholders with order details.

Support

If you need help configuring the Logic App connectors or importing the template, open an issue in this repository with a description of the environment and connectors used.
