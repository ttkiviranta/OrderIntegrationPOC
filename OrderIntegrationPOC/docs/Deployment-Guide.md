# Order Integration POC - Deployment Guide

## Overview

This guide covers deploying the Order Integration POC to Azure production environment.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Step 1: Prepare Azure Resources](#step-1-prepare-azure-resources)
- [Step 2: Configure Application](#step-2-configure-application)
- [Step 3: Deploy Azure Function](#step-3-deploy-azure-function)
- [Step 4: Set Up Logic App](#step-4-set-up-logic-app)
- [Step 5: Configure Monitoring](#step-5-configure-monitoring)
- [Step 6: Verify Deployment](#step-6-verify-deployment)

## Prerequisites

### Required Tools
```powershell
# Azure CLI
az --version  # Must be latest version

# Azure Functions Core Tools
func --version  # Must be 4.x or later

# .NET 8 SDK
dotnet --version  # Must be 8.0.x or later
```

### Azure Subscription
- Active Azure subscription
- Sufficient permissions to create resources
- Resource group created (e.g., "order-integration-poc")

### Azure Credentials
```powershell
# Login to Azure
az login

# Set default subscription
az account set --subscription "Your-Subscription-ID"
```

---

## Step 1: Prepare Azure Resources

### 1.1 Create Resource Group (if not exists)

```powershell
$resourceGroupName = "order-integration-poc"
$location = "North Europe"  # or your preferred location

az group create `
	--name $resourceGroupName `
	--location $location

Write-Host "Resource Group created: $resourceGroupName" -ForegroundColor Green
```

### 1.2 Create Storage Account

```powershell
$storageAccountName = "orderstorstorage$(Get-Random -Minimum 1000 -Maximum 9999)"
$resourceGroupName = "order-integration-poc"

az storage account create `
	--name $storageAccountName `
	--resource-group $resourceGroupName `
	--location "North Europe" `
	--sku "Standard_LRS" `
	--kind "StorageV2"

# Get connection string
$storageKey = az storage account keys list `
	--resource-group $resourceGroupName `
	--account-name $storageAccountName `
	--query "[0].value" -o tsv

$storageConnStr = "DefaultEndpointsProtocol=https;AccountName=$storageAccountName;AccountKey=$storageKey;EndpointSuffix=core.windows.net"

Write-Host "Storage Account created: $storageAccountName" -ForegroundColor Green
Write-Host "Connection String: $storageConnStr" -ForegroundColor Yellow
```

### 1.3 Create Service Bus Namespace

```powershell
$serviceBusNamespace = "order-integration-sb$(Get-Random -Minimum 1000 -Maximum 9999)"
$resourceGroupName = "order-integration-poc"

# Create Service Bus namespace
az servicebus namespace create `
	--resource-group $resourceGroupName `
	--name $serviceBusNamespace `
	--location "North Europe" `
	--sku Standard

# Create queue
az servicebus queue create `
	--resource-group $resourceGroupName `
	--namespace-name $serviceBusNamespace `
	--name "orders-incoming" `
	--lock-duration PT5M `
	--max-delivery-count 10

# Get connection string
$serviceBusConnStr = az servicebus namespace authorization-rule keys list `
	--resource-group $resourceGroupName `
	--namespace-name $serviceBusNamespace `
	--name "RootManageSharedAccessKey" `
	--query "primaryConnectionString" -o tsv

Write-Host "Service Bus namespace created: $serviceBusNamespace" -ForegroundColor Green
Write-Host "Connection String: $serviceBusConnStr" -ForegroundColor Yellow
```

### 1.4 Create Application Insights

```powershell
$appInsightsName = "order-insights-$(Get-Random -Minimum 1000 -Maximum 9999)"
$resourceGroupName = "order-integration-poc"

az monitor app-insights component create `
	--app $appInsightsName `
	--location "North Europe" `
	--resource-group $resourceGroupName `
	--application-type web

# Get instrumentation key
$instrumentationKey = az monitor app-insights component show `
	--app $appInsightsName `
	--resource-group $resourceGroupName `
	--query "instrumentationKey" -o tsv

Write-Host "Application Insights created: $appInsightsName" -ForegroundColor Green
Write-Host "Instrumentation Key: $instrumentationKey" -ForegroundColor Yellow
```

### 1.5 Create Azure Function App

```powershell
$functionAppName = "order-process-func-$(Get-Random -Minimum 1000 -Maximum 9999)"
$storageAccountName = "orderstorstorage..."  # From step 1.2
$resourceGroupName = "order-integration-poc"

# Create App Service Plan
$appServicePlanName = "order-integration-plan"

az appservice plan create `
	--name $appServicePlanName `
	--resource-group $resourceGroupName `
	--sku B1 `
	--is-linux

# Create Function App
az functionapp create `
	--name $functionAppName `
	--resource-group $resourceGroupName `
	--storage-account $storageAccountName `
	--plan $appServicePlanName `
	--runtime dotnet-isolated `
	--runtime-version 8 `
	--functions-version 4 `
	--os-type Linux

Write-Host "Azure Function App created: $functionAppName" -ForegroundColor Green
```

---

## Step 2: Configure Application

### 2.1 Update Application Settings

```powershell
$functionAppName = "order-process-func-..."  # From above
$instrumentationKey = "..."  # From Application Insights
$serviceBusConnStr = "..."  # From Service Bus
$resourceGroupName = "order-integration-poc"

# Set application settings
az functionapp config appsettings set `
	--name $functionAppName `
	--resource-group $resourceGroupName `
	--settings `
		APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey `
		ServiceBusConnection=$serviceBusConnStr `
		ServiceBusQueueName="orders-incoming" `
		ENVIRONMENT="Production"

Write-Host "Application settings configured" -ForegroundColor Green
```

### 2.2 Enable Application Insights

```powershell
$functionAppName = "order-process-func-..."
$resourceGroupName = "order-integration-poc"

az functionapp config appsettings set `
	--name $functionAppName `
	--resource-group $resourceGroupName `
	--settings `
		"ENABLE_FUNCTION_HEALTH_MONITORING=true"

Write-Host "Application Insights enabled" -ForegroundColor Green
```

---

## Step 3: Deploy Azure Function

### 3.1 Build for Release

```powershell
cd "C:\Users\ttkiv\source\repos\OrderIntegrationPOC\OrderFunctionApp"

# Build for release
dotnet publish -c Release -o "./publish"

Write-Host "Build completed" -ForegroundColor Green
```

### 3.2 Deploy Function App

#### Option A: Using Azure Functions Core Tools

```powershell
cd "C:\Users\ttkiv\source\repos\OrderIntegrationPOC\OrderFunctionApp"

# Publish directly
func azure functionapp publish "order-process-func-YOUR_NUMBER"

# You'll be prompted to login if not already authenticated
```

#### Option B: Using ZIP Deploy

```powershell
$functionAppName = "order-process-func-..."
$resourceGroupName = "order-integration-poc"
$publishFolder = "C:\Users\ttkiv\source\repos\OrderIntegrationPOC\OrderFunctionApp\publish"

# Create ZIP file
cd $publishFolder
Compress-Archive -Path "./*" -DestinationPath "..\deploy.zip" -Force

# Deploy ZIP
az functionapp deployment source config-zip `
	--name $functionAppName `
	--resource-group $resourceGroupName `
	--src "..\deploy.zip"

Write-Host "Function App deployed successfully" -ForegroundColor Green
```

### 3.3 Verify Deployment

```powershell
$functionAppName = "order-process-func-..."
$resourceGroupName = "order-integration-poc"

# Get function app properties
az functionapp show `
	--name $functionAppName `
	--resource-group $resourceGroupName

# Get function URL
$functionUrl = az functionapp function show `
	--name $functionAppName `
	--function-name "ProcessOrder" `
	--resource-group $resourceGroupName `
	--query "invoke_url_template" -o tsv

Write-Host "Function URL: $functionUrl" -ForegroundColor Yellow
```

---

## Step 4: Set Up Logic App

### 4.1 Create Logic App Resource

```powershell
$logicAppName = "order-integration-workflow-$(Get-Random -Minimum 1000 -Maximum 9999)"
$resourceGroupName = "order-integration-poc"

az logicapp create `
	--name $logicAppName `
	--resource-group $resourceGroupName `
	--location "North Europe" `
	--definition "@docs/LogicApp-Definition.json"

Write-Host "Logic App created: $logicAppName" -ForegroundColor Green
```

### 4.2 Configure Connections

Open Azure Portal and navigate to your Logic App:

1. Click **Connections** or **API Connections**
2. Set up Service Bus connection:
   - Click **New Connection** if needed
   - Select **Service Bus**
   - Provide connection string from step 1.3
3. Set up Application Insights connection:
   - Click **New Connection**
   - Select **Application Insights**
   - Select your Application Insights instance
4. Update HTTP connector settings:
   - Update the ProcessOrder function URL with actual Function App URL

### 4.3 Deploy Logic App Definition

```powershell
$logicAppName = "order-integration-workflow-..."
$resourceGroupName = "order-integration-poc"

# Update Logic App definition
az logicapp definition update `
	--logic-app-name $logicAppName `
	--resource-group $resourceGroupName `
	--definition "@docs/LogicApp-Definition.json"

Write-Host "Logic App definition updated" -ForegroundColor Green
```

---

## Step 5: Configure Monitoring

### 5.1 Enable Diagnostics Logging

```powershell
$logicAppName = "order-integration-workflow-..."
$appInsightsName = "order-insights-..."
$resourceGroupName = "order-integration-poc"

# Get Application Insights resource ID
$appInsightsId = az monitor app-insights component show `
	--app $appInsightsName `
	--resource-group $resourceGroupName `
	--query "id" -o tsv

# Enable diagnostics
az monitor diagnostic-settings create `
	--name "order-integration-logs" `
	--resource "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$resourceGroupName/providers/Microsoft.Logic/workflows/$logicAppName" `
	--logs '[{"category":"WorkflowRuntime","enabled": true}]' `
	--metrics '[{"category":"AllMetrics","enabled":true}]' `
	--workspace $appInsightsId

Write-Host "Diagnostics logging enabled" -ForegroundColor Green
```

### 5.2 Create Alerts

```powershell
# Alert for high error rate
az monitor metrics alert create `
	--name "order-processing-errors" `
	--resource-group "order-integration-poc" `
	--scopes "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/order-integration-poc/providers/microsoft.insights/components/order-insights-..." `
	--condition "avg 'exceptionsPerSecond' > 1" `
	--window-size PT5M `
	--evaluation-frequency PT1M `
	--action email-action

Write-Host "Alert created" -ForegroundColor Green
```

---

## Step 6: Verify Deployment

### 6.1 Test Function Endpoint

```powershell
$functionUrl = "https://order-process-func-...azurewebsites.net/api/orders/process?code=..."

$body = @{
	orderId = "ORD-PROD-001"
	customerId = "CUST-001"
	total = 199.99
} | ConvertTo-Json

$response = Invoke-RestMethod `
	-Uri $functionUrl `
	-Method Post `
	-ContentType "application/json" `
	-Body $body

Write-Host "Response: $($response | ConvertTo-Json)" -ForegroundColor Green
```

### 6.2 Test Logic App

1. Navigate to Logic App in Azure Portal
2. Click **Trigger** or **Run**
3. Provide test payload for manual trigger
4. Verify it calls the Function App and logs results

### 6.3 Check Application Insights

```powershell
$appInsightsName = "order-insights-..."
$resourceGroupName = "order-integration-poc"

# View recent requests
az monitor app-insights query `
	--app $appInsightsName `
	--analytics-query "requests | take 10"
```

---

## Monitoring and Maintenance

### Regular Health Checks

```powershell
# Check function app status
az functionapp show `
	--name "order-process-func-..." `
	--resource-group "order-integration-poc" `
	--query "state"

# Check Logic App runs
az logicapp run list `
	--logic-app-name "order-integration-workflow-..." `
	--resource-group "order-integration-poc"

# View errors in past hour
az monitor metrics list-definitions `
	--resource "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/order-integration-poc/providers/Microsoft.Web/sites/order-process-func-..."
```

### Scaling Configuration

```powershell
# Scale up App Service Plan if needed
az appservice plan update `
	--name "order-integration-plan" `
	--resource-group "order-integration-poc" `
	--sku "S1"  # Standard tier for higher throughput

Write-Host "App Service Plan scaled" -ForegroundColor Green
```

---

## Cleanup

To delete all resources:

```powershell
$resourceGroupName = "order-integration-poc"

# Delete entire resource group
az group delete `
	--name $resourceGroupName `
	--yes

Write-Host "Resource group deleted" -ForegroundColor Yellow
```

---

## Troubleshooting Deployment Issues

### Function App Won't Start
- Check Application Insights key is valid
- Verify storage account connection string
- Review deployment logs: `az functionapp deployment source show-logs`

### Logic App Not Triggering
- Verify Service Bus connection is active
- Check trigger configuration in Logic App Designer
- Review run history for errors

### High Latency
- Consider upgrading App Service Plan to higher tier
- Enable caching if applicable
- Review function code for performance bottlenecks

---

## Success Checklist

- [ ] All Azure resources created
- [ ] Function App deployed successfully
- [ ] Logic App configured and enabled
- [ ] Application Insights receiving telemetry
- [ ] Test message processed successfully
- [ ] Alerts configured and working
- [ ] Logs visible in Application Insights

---

**Deployment Complete!** 🚀

Your Order Integration POC is now running in Azure production environment.
