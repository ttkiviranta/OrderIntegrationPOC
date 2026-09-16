# OrderIntegrationPOC Infrastructure as Code Guide

This directory contains Bicep infrastructure-as-code templates for deploying the OrderIntegrationPOC solution to Azure with security best practices and enterprise patterns.

## Overview

The Bicep templates in this folder deploy:

1. **Key Vault** - Secure storage for secrets and connection strings
2. **API Management (APIM)** - API gateway with security policies
3. **Private Endpoints** - Network security for database and storage access
4. **Network Security** - TLS 1.2+, HTTPS enforced, security headers

## Files

- `main.bicep` - Main orchestration template (deploy this file)
- `key-vault.bicep` - Key Vault resource with secrets
- `apim.bicep` - API Management service with security policies
- `private-endpoint.bicep` - Private endpoint configuration template
- `INFRASTRUCTURE.md` - This file

## Prerequisites

- Azure CLI 2.20+ with Bicep support
- Azure subscription
- Permissions to create resources in the resource group
- (Optional) Virtual network for private endpoints

## Deployment Steps

### 1. Create Resource Group

```powershell
$resourceGroupName = "rg-orderintegrationpoc-dev"
$location = "westeurope"

az group create `
  --name $resourceGroupName `
  --location $location
```

### 2. Deploy Infrastructure with Main Template

```powershell
$projectName = "orderintegrationpoc"
$environment = "dev"
$apimPublisherEmail = "admin@yourcompany.com"

az deployment group create `
  --resource-group $resourceGroupName `
  --template-file bicep/main.bicep `
  --parameters `
	projectName=$projectName `
	environment=$environment `
	location=$location `
	apimPublisherEmail=$apimPublisherEmail
```

### 3. Configure Key Vault Secrets

After deployment, populate the Key Vault secrets:

```powershell
$keyVaultName = "orderintegrationpoc-kv-dev"

# SQL Connection String
az keyvault secret set `
  --vault-name $keyVaultName `
  --name "SqlConnectionString" `
  --value "Server=<server>.database.windows.net;Database=OrderIntegrationPOC_DB;User Id=<user>;Password=<password>;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;"

# Service Bus Connection String
az keyvault secret set `
  --vault-name $keyVaultName `
  --name "ServiceBusConnection" `
  --value "Endpoint=sb://<namespace>.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=<key>"

# Application Insights Connection String
az keyvault secret set `
  --vault-name $keyVaultName `
  --name "ApplicationInsightsConnectionString" `
  --value "InstrumentationKey=<key>;IngestionEndpoint=<endpoint>"
```

### 4. Grant Function App Access to Key Vault

After deploying the Function App, grant it access to Key Vault using Managed Identity:

```powershell
$functionAppName = "orderfunc-dev"
$functionAppResourceGroup = $resourceGroupName

# Get the Function App's managed identity principal ID
$principalId = az functionapp identity show `
  --resource-group $functionAppResourceGroup `
  --name $functionAppName `
  --query principalId `
  --output tsv

# Grant Key Vault access
az keyvault set-policy `
  --vault-name $keyVaultName `
  --object-id $principalId `
  --secret-permissions get list `
  --key-permissions get list
```

### 5. Configure Function App Settings

Set the Key Vault URL in the Function App configuration:

```powershell
$keyVaultUri = az keyvault show `
  --resource-group $resourceGroupName `
  --name $keyVaultName `
  --query properties.vaultUri `
  --output tsv

az functionapp config appsettings set `
  --resource-group $functionAppResourceGroup `
  --name $functionAppName `
  --settings KeyVaultUrl=$keyVaultUri
```

## Security Best Practices Implemented

### 1. Key Vault
- Network ACLs: Deny by default, Allow AzureServices
- System-assigned managed identity for secure access
- Automatic secret versioning for audit trail
- Encryption at rest (default)

### 2. API Management
- TLS 1.2+ enforced
- Security headers configured (HSTS, X-Frame-Options, X-Content-Type-Options)
- OAuth 2.0 support for API authorization
- Logging to Application Insights
- Rate limiting and throttling policies (can be configured per API)

### 3. Network Security
- Private endpoints for database and storage (template provided)
- All traffic encrypted in transit
- No public endpoints exposed for sensitive resources

### 4. Identity & Access
- Managed Identity for Azure Functions
- Role-based access control (RBAC) for all resources
- Service Principal support for CI/CD pipelines
- Key Vault access policies with least privilege principle

## Configuration for Production

### Recommended Changes for Production:

1. **APIM SKU**: Change from "Standard" to "Premium" for production workloads
   ```bicep
   param apimSku string = 'Premium'
   ```

2. **Key Vault**: Enable private endpoints
   ```bicep
   networkAcls: {
	 defaultAction: 'Deny'
	 bypass: 'AzureServices'
   }
   ```

3. **Virtual Network Integration**:
   - Deploy APIM in virtual network (set `virtualNetworkType: 'External'`)
   - Create private endpoints for SQL Database and Storage Account
   - Set up Azure Firewall or NSG rules for network policy

4. **Monitoring & Diagnostics**:
   - Enable diagnostic logs for APIM, Key Vault, and Functions
   - Configure Log Analytics for centralized monitoring
   - Set up alerts for security events

5. **Secrets Management**:
   - Use Azure DevOps or GitHub Actions for secret deployment
   - Implement secret rotation policies
   - Enable soft delete and purge protection on Key Vault

6. **Compliance**:
   - Enable audit logging for all resource changes
   - Implement resource locks to prevent accidental deletion
   - Use Azure Policy for governance

## Deploying Private Endpoints

To deploy private endpoints for SQL Database:

```powershell
$sqlServerName = "orderintegrationpoc-sql-dev"
$virtualNetworkName = "vnet-orderintegrationpoc-dev"
$subnetName = "subnet-databases"

az deployment group create `
  --resource-group $resourceGroupName `
  --template-file bicep/private-endpoint.bicep `
  --parameters `
	privateEndpointName="pe-${sqlServerName}" `
	resourceName=$sqlServerName `
	resourceGroupName=$resourceGroupName `
	serviceType="sqlServer" `
	virtualNetworkId="/subscriptions/<subscription-id>/resourceGroups/${resourceGroupName}/providers/Microsoft.Network/virtualNetworks/${virtualNetworkName}" `
	subnetName=$subnetName `
	location=$location
```

## Troubleshooting

### Key Vault Access Issues
```powershell
# Test Key Vault access
az keyvault secret list --vault-name $keyVaultName

# Check Managed Identity permissions
az keyvault show --vault-name $keyVaultName --query properties.accessPolicies
```

### APIM Connectivity Issues
```powershell
# Check APIM status
az apim show --resource-group $resourceGroupName --name $apiManagementName

# View diagnostic logs
az apim diagnostics list --resource-group $resourceGroupName --name $apiManagementName
```

### Private Endpoint Issues
```powershell
# Check private endpoint status
az network private-endpoint show `
  --resource-group $resourceGroupName `
  --name "pe-<resource-name>"

# Test DNS resolution
nslookup <database-name>.database.windows.net
```

## Cost Optimization

For development environments, consider:
1. Using Key Vault with standard SKU
2. Deploying APIM with Developer SKU (single replica)
3. Disabling unused features in APIM
4. Using Azure Database for SQL with single node
5. Implementing auto-shutdown for non-production resources

For production environments:
1. Implement Azure Cost Management budgets and alerts
2. Use Reserved Instances for predictable workloads
3. Configure autoscaling for Functions and APIM
4. Implement data retention policies for logs

## Support & Documentation

- [Azure Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [Azure Key Vault Documentation](https://learn.microsoft.com/azure/key-vault/)
- [Azure API Management Documentation](https://learn.microsoft.com/azure/api-management/)
- [Azure Private Endpoints Documentation](https://learn.microsoft.com/azure/private-link/private-endpoint-overview)
