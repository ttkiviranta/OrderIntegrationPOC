Deploying the Logic App and API Connections (Bicep)

This document describes how to deploy the Logic App template and API connections that automate creation of the Logic App and the required connectors (SQL, Office 365 Outlook, Microsoft Teams).

Prerequisites

- Azure CLI (az) installed and logged in: `az login`
- Bicep CLI or Azure CLI with Bicep support (Azure CLI >= 2.20 includes Bicep)
- A resource group to deploy into (create one if needed)
- SQL Server credentials (SQL Authentication) for the database used by EF Core
- Team and Channel identifiers (TeamId and ChannelId) for Teams posting
- After deployment you will typically need to authorize the Office 365 and Teams API connections interactively in the Azure Portal

Files in this folder

- order-notification.json  - Logic App workflow definition used by the deployment
- deploy.bicep             - Bicep template that creates API connections and the Logic App
- DEPLOY.md                - This deployment guide

Deployment steps (example)

1. Create a resource group (if you don't have one already):

```powershell
az group create --name rg-orderintegrationpoc --location westeurope
```

2. Deploy the Bicep template

Replace parameter values below with your environment-specific values. Do NOT include production secrets in source control.

```powershell
az deployment group create \
  --resource-group rg-orderintegrationpoc \
  --template-file logicapps/deploy.bicep \
  --parameters \
	logicAppName="order-notification-la" \
	sqlConnectionName="sql-connection-orderintegrationpoc" \
	sqlServer="your-sql-server.database.windows.net" \
	sqlDatabase="OrderIntegrationPOC_DB" \
	sqlUsername="dbuser" \
	sqlPassword="<your-db-password>" \
	officeConnectionName="office365-connection-orderintegrationpoc" \
	teamsConnectionName="teams-connection-orderintegrationpoc" \
	recipientEmail="alerts@example.com" \
	teamId="<your-team-id>" \
	channelId="<your-channel-id>"
```

Notes:
- The deployment will create three API connection resources and the Logic App workflow.
- Office 365 and Teams connections typically require interactive OAuth consent; after deployment open the connection resource in the Azure Portal and sign in / authorize the connection.
- The SQL connection is created with SQL Authentication parameters provided to the template. Use a least-privilege SQL user.

3. Authorize API connections

- In the Azure Portal, navigate to the resource group and open the newly created connections (type: API Connection). Select the Office 365 connection and follow the sign-in flow to authorize. Repeat for the Teams connection.

4. Verify Logic App

- Open the Logic App resource (type: Workflow) in the Azure Portal.
- In the Logic App Designer confirm that the workflow definition imported successfully and that the workflow parameters point to the created connection resource ids.
- If needed, re-save the Logic App in the designer to refresh connector references.

5. Test end-to-end

- Insert a test order into the Orders table (or let the existing Function pipeline insert a real test order). The Logic App trigger is configured to poll the Orders table every minute by default.
- Verify you receive an email and a Teams Adaptive Card in the configured channel.

Troubleshooting

- If the Logic App does not trigger, check the run history and the SQL connection test in the connection resource.
- If the Office 365 or Teams actions fail with authentication errors, authorize the connections in the portal and ensure the account used has necessary permissions.
- For high-throughput scenarios consider replacing polling with an event-driven approach (Event Grid / Service Bus).

Security

- Do not store production credentials in the repository. Use Azure Key Vault or parameterize deployments in CI/CD pipelines (GitHub Actions, Azure DevOps) and provide secrets at deployment time.

Support

If you have issues deploying the template, open an issue in this repository including the deployment command used and any error messages from `az`.
