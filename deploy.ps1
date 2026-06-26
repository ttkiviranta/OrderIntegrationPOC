# ============================================================================
# Order Integration POC - Azure Deployment Script
# ============================================================================
# This script automates the creation of all Azure resources needed for the
# Order Integration Proof of Concept solution.
#
# Prerequisites:
#   - Azure CLI installed (az --version)
#   - Azure subscription active (az login)
#
# Usage:
#   .\deploy.ps1
#
# Created: January 2024
# Author: Timo Kiviranta
# ============================================================================

# Enable strict error handling
$ErrorActionPreference = "Stop"

# ============================================================================
# CONFIGURATION
# ============================================================================

Write-Host @"
╔════════════════════════════════════════════════════════════════════════════╗
║                    ORDER INTEGRATION POC - DEPLOYMENT                      ║
║                          Azure Resource Setup                              ║
╚════════════════════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan

Write-Host "Initializing configuration..." -ForegroundColor Yellow

# Resource naming
$resourceGroup = "order-integration-poc"
$location = "North Europe"
$storageAccount = "orderstorage$((Get-Random -Min 10000 -Max 99999))"
$serviceBusNamespace = "orderbus-$((Get-Random -Min 10000 -Max 99999))"
$appInsights = "order-insights-$((Get-Random -Min 10000 -Max 99999))"
$functionApp = "order-process-$((Get-Random -Min 10000 -Max 99999))"
$appServicePlan = "order-plan-$((Get-Random -Min 10000 -Max 99999))"

Write-Host @"
Configuration:
  Resource Group: $resourceGroup
  Location: $location
  Storage Account: $storageAccount
  Service Bus: $serviceBusNamespace
  Function App: $functionApp
  App Service Plan: $appServicePlan
  Application Insights: $appInsights
"@ -ForegroundColor Green

Read-Host "Press Enter to continue or Ctrl+C to cancel"

# ============================================================================
# STEP 1: CREATE RESOURCE GROUP
# ============================================================================

Write-Host "`n[1/7] Creating Resource Group..." -ForegroundColor Cyan

try {
	az group create `
		--name $resourceGroup `
		--location $location | Out-Null
	Write-Host "✓ Resource Group created: $resourceGroup" -ForegroundColor Green
}
catch {
	Write-Host "✗ Error creating Resource Group: $_" -ForegroundColor Red
	exit 1
}

# ============================================================================
# STEP 2: CREATE STORAGE ACCOUNT
# ============================================================================

Write-Host "`n[2/7] Creating Storage Account..." -ForegroundColor Cyan

try {
	az storage account create `
		--name $storageAccount `
		--resource-group $resourceGroup `
		--location $location `
		--sku "Standard_LRS" `
		--kind "StorageV2" `
		--https-only true | Out-Null
	Write-Host "✓ Storage Account created: $storageAccount" -ForegroundColor Green
}
catch {
	Write-Host "✗ Error creating Storage Account: $_" -ForegroundColor Red
	exit 1
}

# ============================================================================
# STEP 3: CREATE SERVICE BUS NAMESPACE
# ============================================================================

Write-Host "`n[3/7] Creating Service Bus Namespace..." -ForegroundColor Cyan

try {
	az servicebus namespace create `
		--resource-group $resourceGroup `
		--name $serviceBusNamespace `
		--location $location `
		--sku "Standard" | Out-Null
	Write-Host "✓ Service Bus Namespace created: $serviceBusNamespace" -ForegroundColor Green
}
catch {
	Write-Host "✗ Error creating Service Bus Namespace: $_" -ForegroundColor Red
	exit 1
}

# ============================================================================
# STEP 4: CREATE SERVICE BUS QUEUE
# ============================================================================

Write-Host "`n[4/7] Creating Service Bus Queue..." -ForegroundColor Cyan

try {
	az servicebus queue create `
		--resource-group $resourceGroup `
		--namespace-name $serviceBusNamespace `
		--name "orders-incoming" `
		--lock-duration "PT5M" `
		--max-delivery-count 10 | Out-Null
	Write-Host "✓ Service Bus Queue created: orders-incoming" -ForegroundColor Green
}
catch {
	Write-Host "✗ Error creating Service Bus Queue: $_" -ForegroundColor Red
	exit 1
}

# ============================================================================
# STEP 5: CREATE APPLICATION INSIGHTS
# ============================================================================

Write-Host "`n[5/7] Creating Application Insights..." -ForegroundColor Cyan

try {
	az monitor app-insights component create `
		--app $appInsights `
		--resource-group $resourceGroup `
		--location $location `
		--application-type "web" | Out-Null

	Write-Host "✓ Application Insights created: $appInsights" -ForegroundColor Green

	# Get instrumentation key
	$instrumentationKey = az monitor app-insights component show `
		--app $appInsights `
		--resource-group $resourceGroup `
		--query "instrumentationKey" -o tsv

	Write-Host "✓ Instrumentation Key retrieved" -ForegroundColor Green
}
catch {
	Write-Host "✗ Error creating Application Insights: $_" -ForegroundColor Red
	exit 1
}

# ============================================================================
# STEP 6: CREATE APP SERVICE PLAN & FUNCTION APP
# ============================================================================

Write-Host "`n[6/7] Creating App Service Plan..." -ForegroundColor Cyan

try {
	az appservice plan create `
		--name $appServicePlan `
		--resource-group $resourceGroup `
		--sku "B1" `
		--is-linux | Out-Null
	Write-Host "✓ App Service Plan created: $appServicePlan" -ForegroundColor Green
}
catch {
	Write-Host "✗ Error creating App Service Plan: $_" -ForegroundColor Red
	exit 1
}

Write-Host "`n[6/7] Creating Function App..." -ForegroundColor Cyan

try {
	az functionapp create `
		--name $functionApp `
		--resource-group $resourceGroup `
		--storage-account $storageAccount `
		--plan $appServicePlan `
		--runtime "dotnet-isolated" `
		--runtime-version "8" `
		--functions-version "4" `
		--os-type "Linux" | Out-Null
	Write-Host "✓ Function App created: $functionApp" -ForegroundColor Green
}
catch {
	Write-Host "✗ Error creating Function App: $_" -ForegroundColor Red
	exit 1
}

# ============================================================================
# STEP 7: GET SERVICE BUS CONNECTION STRING & CONFIGURE APP SETTINGS
# ============================================================================

Write-Host "`n[7/7] Configuring Function App Settings..." -ForegroundColor Cyan

try {
	# Get Service Bus connection string
	$serviceBusConnStr = az servicebus namespace authorization-rule keys list `
		--resource-group $resourceGroup `
		--namespace-name $serviceBusNamespace `
		--name "RootManageSharedAccessKey" `
		--query "primaryConnectionString" -o tsv

	Write-Host "✓ Service Bus connection string retrieved" -ForegroundColor Green

	# Configure application settings
	az functionapp config appsettings set `
		--name $functionApp `
		--resource-group $resourceGroup `
		--settings `
			"APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" `
			"ServiceBusConnection=$serviceBusConnStr" `
			"ServiceBusQueueName=orders-incoming" `
			"ENVIRONMENT=Production" | Out-Null

	Write-Host "✓ Application settings configured" -ForegroundColor Green
}
catch {
	Write-Host "✗ Error configuring settings: $_" -ForegroundColor Red
	exit 1
}

# ============================================================================
# DEPLOYMENT SUMMARY
# ============================================================================

# Get Function App function details
$functionUrl = az functionapp function show `
	--name $functionApp `
	--function-name "ProcessOrder" `
	--resource-group $resourceGroup `
	--query "invoke_url_template" -o tsv 2>/dev/null || "URL will be available after code deployment"

Write-Host @"

╔════════════════════════════════════════════════════════════════════════════╗
║                         DEPLOYMENT SUCCESSFUL! ✓                          ║
╚════════════════════════════════════════════════════════════════════════════╝

Summary of Created Resources:
─────────────────────────────────────────────────────────────────────────────

📦 RESOURCE GROUP
  Name: $resourceGroup
  Location: $location

💾 STORAGE ACCOUNT
  Name: $storageAccount
  SKU: Standard_LRS
  Kind: StorageV2

🚌 SERVICE BUS
  Namespace: $serviceBusNamespace
  Queue: orders-incoming
  SKU: Standard
  Connection String: (Configured in Function App)

⚡ FUNCTION APP
  Name: $functionApp
  Runtime: .NET 8
  Plan: $appServicePlan
  OS: Linux

📊 APPLICATION INSIGHTS
  Name: $appInsights
  Instrumentation Key: $instrumentationKey

─────────────────────────────────────────────────────────────────────────────

Next Steps:
───────────

1. Deploy the ProcessOrder function:

   cd OrderIntegrationPOC\OrderFunctionApp
   dotnet publish -c Release
   func azure functionapp publish $functionApp

2. Test the Function App:

   `$url = "$functionUrl"
   `$body = @{
	   orderId = "TEST-001"
	   customerId = "CUST-001"
	   total = 99.99
   } | ConvertTo-Json

   Invoke-RestMethod -Uri `$url -Method Post -ContentType "application/json" -Body `$body

3. Create Logic App (manual via Azure Portal):
   - Trigger: Service Bus - "When a message is received in a queue"
   - Action: HTTP POST to Function App
   - Action: Log to Application Insights

4. Send test message to Service Bus:

   `$connectionString = "$serviceBusConnStr"
   (Use Service Bus Explorer or Azure Portal)

─────────────────────────────────────────────────────────────────────────────

Resource Management:
──────────────────

View Resources:
  az resource list --resource-group $resourceGroup

Delete All Resources:
  az group delete --name $resourceGroup --yes

Monitor Logs:
  az monitor app-insights query --app $appInsights --analytics-query "requests | take 10"

─────────────────────────────────────────────────────────────────────────────

Documentation:
──────────────
  README: ./README.md
  Testing Guide: ./OrderIntegrationPOC/docs/Testing-Guide.md
  Deployment Guide: ./OrderIntegrationPOC/docs/Deployment-Guide.md
  API Reference: ./OrderIntegrationPOC/docs/API-Reference.md

═════════════════════════════════════════════════════════════════════════════

@ -ForegroundColor Green

# Save configuration to file for later reference
$config = @{
	ResourceGroup = $resourceGroup
	Location = $location
	StorageAccount = $storageAccount
	ServiceBusNamespace = $serviceBusNamespace
	ServiceBusQueue = "orders-incoming"
	FunctionApp = $functionApp
	AppServicePlan = $appServicePlan
	AppInsights = $appInsights
	InstrumentationKey = $instrumentationKey
	ServiceBusConnectionString = $serviceBusConnStr
	DeploymentDate = Get-Date
}

# Save to JSON file
$configPath = "azure-deployment-config.json"
$config | ConvertTo-Json | Out-File -FilePath $configPath -Force

Write-Host "`n✓ Configuration saved to: $configPath" -ForegroundColor Green
Write-Host "`nDeployment completed successfully! 🎉" -ForegroundColor Cyan
