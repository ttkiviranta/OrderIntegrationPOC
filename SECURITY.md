# Security Hardening Guide for OrderIntegrationPOC

This document outlines the security best practices and configurations for the OrderIntegrationPOC solution in production environments.

## Table of Contents

1. [Key Vault Configuration](#key-vault-configuration)
2. [Managed Identity Setup](#managed-identity-setup)
3. [Network Security](#network-security)
4. [TLS and Encryption](#tls-and-encryption)
5. [API Security (APIM)](#api-security-apim)
6. [Database Security](#database-security)
7. [Secrets Management](#secrets-management)
8. [Monitoring and Logging](#monitoring-and-logging)
9. [Incident Response](#incident-response)
10. [Compliance Checklist](#compliance-checklist)

---

## Key Vault Configuration

### Overview
Azure Key Vault is used to store and manage all sensitive configuration data including connection strings, API keys, and certificates.

### Setup Steps

#### 1. Create Key Vault with Secure Settings

```powershell
$vaultName = "orderintegrationpoc-kv-prod"
$resourceGroup = "rg-orderintegrationpoc-prod"
$location = "westeurope"

az keyvault create `
  --resource-group $resourceGroup `
  --name $vaultName `
  --location $location `
  --enable-rbac-authorization true `
  --enable-soft-delete true `
  --soft-delete-retention-days 90 `
  --enable-purge-protection true
```

#### 2. Configure Network Access

```powershell
# Deny all public access by default
az keyvault update `
  --resource-group $resourceGroup `
  --name $vaultName `
  --default-action Deny `
  --bypass AzureServices
```

#### 3. Store Secrets

```powershell
# SQL Connection String (use connection string with encryption)
az keyvault secret set `
  --vault-name $vaultName `
  --name "SqlConnectionString" `
  --value "Server=<server>.database.windows.net;Database=OrderIntegrationPOC_DB;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;Authentication=Active Directory Default;"

# Service Bus Connection (use SAS token with minimal permissions)
az keyvault secret set `
  --vault-name $vaultName `
  --name "ServiceBusConnection" `
  --value "Endpoint=sb://<namespace>.servicebus.windows.net/;SharedAccessKeyName=<policy-name>;SharedAccessKey=<key>;TransportType=Amqp"

# Application Insights
az keyvault secret set `
  --vault-name $vaultName `
  --name "ApplicationInsightsConnectionString" `
  --value "InstrumentationKey=<key>;IngestionEndpoint=<endpoint>;AuthenticationApiKey=<api-key>"
```

### Best Practices

- **Enable Audit Logging**: Monitor access to secrets
- **Implement Key Rotation**: Rotate secrets every 90 days
- **Use Separate Vaults**: Development, staging, and production should have separate vaults
- **Enable Purge Protection**: Prevent accidental deletion of critical secrets
- **Limit Access**: Use RBAC with least privilege principle

---

## Managed Identity Setup

### Overview
Managed Identity eliminates the need for credentials in code and environment variables.

### Azure Functions - System-Assigned Identity

#### 1. Enable Managed Identity

```powershell
$functionAppName = "orderfunc-prod"
$resourceGroup = "rg-orderintegrationpoc-prod"

az functionapp identity assign `
  --resource-group $resourceGroup `
  --name $functionAppName `
  --identities "[system]"
```

#### 2. Grant Key Vault Access

```powershell
$keyVaultName = "orderintegrationpoc-kv-prod"

# Get the function app's managed identity principal ID
$principalId = az functionapp identity show `
  --resource-group $resourceGroup `
  --name $functionAppName `
  --query principalId `
  --output tsv

# Grant access using RBAC
az role assignment create `
  --role "Key Vault Secrets User" `
  --assignee-object-id $principalId `
  --assignee-principal-type ServicePrincipal `
  --scope /subscriptions/<subscription-id>/resourcegroups/$resourceGroup/providers/Microsoft.KeyVault/vaults/$keyVaultName
```

#### 3. Update Application Configuration

In `Program.cs`:
```csharp
var keyVaultUrl = builder.Configuration["KeyVaultUrl"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
	builder.Configuration.AddAzureKeyVault(
		new Uri(keyVaultUrl),
		new DefaultAzureCredential());
}
```

#### 4. Update Function App Settings

```powershell
az functionapp config appsettings set `
  --resource-group $resourceGroup `
  --name $functionAppName `
  --settings `
	KeyVaultUrl="https://$keyVaultName.vault.azure.net/" `
	AZURE_CLIENT_ID="" `  # Optional: explicitly specify if needed
	AZURE_TENANT_ID="" `  # Optional: explicitly specify if needed
	AZURE_CLIENT_SECRET="" `  # Do NOT use with Managed Identity
```

### Benefits

- **No Secret Storage in Code**: Credentials not hardcoded
- **Automatic Token Management**: Azure SDK handles token refresh
- **Audit Trail**: Access is logged and auditable
- **Rotation Support**: Secrets can be rotated without code changes

---

## Network Security

### Private Endpoints

#### Deploy Private Endpoints for SQL Database

```powershell
$sqlServerName = "orderintegrationpoc-sql-prod"
$resourceGroup = "rg-orderintegrationpoc-prod"

# Create private endpoint
az network private-endpoint create `
  --resource-group $resourceGroup `
  --name "pe-sql-$sqlServerName" `
  --vnet-name "vnet-orderintegrationpoc-prod" `
  --subnet "subnet-databases" `
  --private-connection-resource-id `/subscriptions/<subscription-id>/resourceGroups/$resourceGroup/providers/Microsoft.Sql/servers/$sqlServerName` `
  --group-ids sqlServer `
  --connection-name "sql-connection"
```

#### Create Private DNS Zone

```powershell
# Create private DNS zone
az network private-dns zone create `
  --resource-group $resourceGroup `
  --name "privatelink.database.windows.net"

# Link to virtual network
az network private-dns link vnet create `
  --resource-group $resourceGroup `
  --zone-name "privatelink.database.windows.net" `
  --name "vnet-link" `
  --virtual-network "vnet-orderintegrationpoc-prod" `
  --registration-enabled false
```

### Network Security Groups (NSG)

```powershell
# Create NSG
az network nsg create `
  --resource-group $resourceGroup `
  --name "nsg-functions-prod"

# Allow inbound HTTPS only
az network nsg rule create `
  --resource-group $resourceGroup `
  --nsg-name "nsg-functions-prod" `
  --name "AllowHTTPSInbound" `
  --priority 100 `
  --source-address-prefixes "VirtualNetwork" `
  --destination-port-ranges 443 `
  --access Allow `
  --protocol Tcp `
  --direction Inbound

# Deny all other inbound
az network nsg rule create `
  --resource-group $resourceGroup `
  --nsg-name "nsg-functions-prod" `
  --name "DenyAllInbound" `
  --priority 1000 `
  --source-address-prefixes "*" `
  --destination-address-prefixes "*" `
  --access Deny `
  --protocol "*" `
  --direction Inbound
```

---

## TLS and Encryption

### Azure SQL Database - Enforce TLS 1.2+

```powershell
$sqlServerName = "orderintegrationpoc-sql-prod"
$resourceGroup = "rg-orderintegrationpoc-prod"

# Enforce minimum TLS 1.2
az sql server update `
  --resource-group $resourceGroup `
  --name $sqlServerName `
  --minimal-tls-version "1.2"

# Disable public endpoint (use private endpoints only)
az sql server firewall-rule create `
  --resource-group $resourceGroup `
  --server $sqlServerName `
  --name "DenyPublicAccess" `
  --start-ip-address "255.255.255.255" `
  --end-ip-address "255.255.255.255"
```

### Service Bus - Enforce TLS 1.2

```powershell
$namespaceName = "orderintegrationpoc-sb-prod"
$resourceGroup = "rg-orderintegrationpoc-prod"

# Update Service Bus namespace
az servicebus namespace update `
  --resource-group $resourceGroup `
  --namespace-name $namespaceName `
  --minimum-tls-version "1.2"
```

### Connection String Examples

```csharp
// SQL Connection String with TLS enforcement
"Server=<server>.database.windows.net;Database=OrderIntegrationPOC_DB;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;Authentication=Active Directory Default;"

// Service Bus with TLS
"Endpoint=sb://<namespace>.servicebus.windows.net/;SharedAccessKeyName=<policy>;SharedAccessKey=<key>;TransportType=Amqp"
```

---

## API Security (APIM)

### Configure APIM Policies

#### 1. OAuth 2.0 Authorization

```xml
<inbound>
	<validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized">
		<openid-config url="https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration" />
		<audiences>
			<audience>api://ordersapi</audience>
		</audiences>
		<issuers>
			<issuer>https://sts.windows.net/{{tenant-id}}/</issuer>
		</issuers>
	</validate-jwt>
</inbound>
```

#### 2. Rate Limiting

```xml
<policies>
	<inbound>
		<rate-limit-by-key calls="100" renewal-period="60" counter-key="@(context.Request.Headers.GetValueOrDefault("Authorization","").AsJwt()?.Subject)" />
	</inbound>
</policies>
```

#### 3. Security Headers

```xml
<outbound>
	<set-header name="X-Content-Type-Options" exists-action="override">
		<value>nosniff</value>
	</set-header>
	<set-header name="X-Frame-Options" exists-action="override">
		<value>DENY</value>
	</set-header>
	<set-header name="Strict-Transport-Security" exists-action="override">
		<value>max-age=31536000; includeSubDomains; preload</value>
	</set-header>
	<set-header name="X-XSS-Protection" exists-action="override">
		<value>1; mode=block</value>
	</set-header>
	<set-header name="Content-Security-Policy" exists-action="override">
		<value>default-src 'self'</value>
	</set-header>
</outbound>
```

---

## Database Security

### Azure SQL Database Recommendations

#### 1. Enable Transparent Data Encryption (TDE)

```powershell
$sqlServerName = "orderintegrationpoc-sql-prod"
$databaseName = "OrderIntegrationPOC_DB"
$resourceGroup = "rg-orderintegrationpoc-prod"

az sql db tde set `
  --resource-group $resourceGroup `
  --server $sqlServerName `
  --database $databaseName `
  --status "Enabled"
```

#### 2. Enable Azure Defender

```powershell
az sql server advanced-threat-protection-setting update `
  --resource-group $resourceGroup `
  --name $sqlServerName `
  --state "On" `
  --email-admins true
```

#### 3. Enable Auditing

```powershell
az sql server audit-policy update `
  --resource-group $resourceGroup `
  --name $sqlServerName `
  --actions "SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP" "FAILED_DATABASE_AUTHENTICATION_GROUP" "BATCH_COMPLETED_GROUP" `
  --state "Enabled" `
  --storage-endpoint "https://<storage-account>.blob.core.windows.net" `
  --storage-key "<storage-key>" `
  --retention-days 90
```

#### 4. Enable Column-Level Encryption for Sensitive Data

In Entity Framework Core:
```csharp
modelBuilder.Entity<Order>()
	.Property(o => o.CustomerId)
	.HasConversion(v => EncryptionService.Encrypt(v), 
				  v => EncryptionService.Decrypt(v));
```

---

## Secrets Management

### Secret Rotation Strategy

#### 1. Implement Rotation for SQL Credentials

```powershell
# Scheduled Azure Function to rotate SQL password
# Triggers every 90 days
# 1. Changes SQL password
# 2. Updates Key Vault with new password
# 3. Logs rotation event
```

#### 2. Use Azure Function Timer Trigger

```csharp
[Function("RotateSecrets")]
public async Task RunAsync(
	[TimerTrigger("0 0 0 1 */3 *")] TimerInfo myTimer,  // Every 90 days
	ILogger log)
{
	// Rotate SQL password
	// Update Key Vault
	// Send notification
}
```

### Access Control

```powershell
# Grant developer read-only access (development only)
az role assignment create `
  --role "Key Vault Secrets Officer" `
  --assignee-object-id "<developer-object-id>" `
  --assignee-principal-type User `
  --scope /subscriptions/<subscription-id>/resourcegroups/$resourceGroup/providers/Microsoft.KeyVault/vaults/$keyVaultName

# Grant production app full access (production)
az role assignment create `
  --role "Key Vault Secrets User" `
  --assignee-object-id "<app-principal-id>" `
  --assignee-principal-type ServicePrincipal `
  --scope /subscriptions/<subscription-id>/resourcegroups/$resourceGroup/providers/Microsoft.KeyVault/vaults/$keyVaultName
```

---

## Monitoring and Logging

### Application Insights Configuration

#### 1. Enable Diagnostic Logs

```powershell
az monitor diagnostic-settings create `
  --resource /subscriptions/<subscription-id>/resourcegroups/$resourceGroup/providers/Microsoft.Web/sites/$functionAppName `
  --name "diagnostics-to-logs" `
  --logs '[{"category":"FunctionAppLogs","enabled":true,"retentionPolicy":{"enabled":true,"days":90}}]' `
  --workspace /subscriptions/<subscription-id>/resourcegroups/$resourceGroup/providers/Microsoft.OperationalInsights/workspaces/$logAnalyticsName
```

#### 2. Create Alert Rules

```powershell
# Alert on failed authentication attempts
az monitor metrics alert create `
  --resource-group $resourceGroup `
  --name "alert-failed-auth" `
  --description "Alert when authentication fails" `
  --scopes /subscriptions/<subscription-id>/resourcegroups/$resourceGroup/providers/Microsoft.KeyVault/vaults/$keyVaultName `
  --condition "total AuthenticationFailure > 5" `
  --window-size 5m `
  --evaluation-frequency 1m `
  --action email <admin-email>
```

#### 3. Log Queries

```kusto
// Track Key Vault access
AzureDiagnostics
| where ResourceType == "VAULTS"
| where OperationName == "SecretGet" or OperationName == "SecretSet"
| summarize count() by IdentityDetails_ObjectId, TimeGenerated

// Track Service Bus errors
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.SERVICEBUS"
| where Level == "Error"
| summarize count() by Message, TimeGenerated
```

---

## Incident Response

### Quick Response Checklist

#### If Secrets Are Compromised

1. **Revoke Immediately**
   ```powershell
   az keyvault secret delete --vault-name $vaultName --name "<secret-name>"
   ```

2. **Create New Secret**
   ```powershell
   az keyvault secret set --vault-name $vaultName --name "<secret-name>" --value "<new-value>"
   ```

3. **Update Applications**
   - Restart Function Apps to refresh configuration
   - Monitor logs for errors

4. **Audit Access**
   ```powershell
   az monitor diagnostic-settings list --resource "/subscriptions/<subscription-id>/resourcegroups/$resourceGroup/providers/Microsoft.KeyVault/vaults/$keyVaultName"
   ```

#### If Unauthorized Access Detected

1. **Check Access Logs**
   ```kusto
   AzureDiagnostics
   | where ResourceType == "VAULTS"
   | where OperationName == "SecretGet"
   | where ResultSignature == "Unauthorized"
   ```

2. **Review Role Assignments**
   ```powershell
   az role assignment list --scope "/subscriptions/<subscription-id>/resourcegroups/$resourceGroup/providers/Microsoft.KeyVault/vaults/$keyVaultName"
   ```

3. **Revoke Suspicious Access**
   ```powershell
   az role assignment delete --assignee-object-id "<suspicious-principal>" --role "Key Vault Secrets User"
   ```

---

## Compliance Checklist

- [ ] **Key Vault**
  - [ ] Network ACLs configured (Deny by default)
  - [ ] Soft delete enabled
  - [ ] Purge protection enabled
  - [ ] Audit logging enabled
  - [ ] All secrets use managed identity access

- [ ] **Database**
  - [ ] TLS 1.2+ enforced
  - [ ] Private endpoints configured
  - [ ] Public endpoints disabled
  - [ ] Transparent Data Encryption enabled
  - [ ] Azure Defender enabled
  - [ ] Audit logging enabled

- [ ] **API Management**
  - [ ] OAuth 2.0 configured
  - [ ] Rate limiting policies applied
  - [ ] Security headers configured
  - [ ] TLS 1.2+ enforced
  - [ ] Logging to Application Insights enabled

- [ ] **Function App**
  - [ ] Managed Identity enabled
  - [ ] Running on App Service Plan (can configure VNet)
  - [ ] HTTPS only enabled
  - [ ] Client certificates optional
  - [ ] Diagnostic logs enabled

- [ ] **Network**
  - [ ] Virtual Network configured
  - [ ] Private Endpoints for all sensitive resources
  - [ ] NSG rules restrictive
  - [ ] No public IPs for sensitive resources
  - [ ] DDoS Protection enabled (optional for Premium)

- [ ] **Secrets Management**
  - [ ] No secrets in configuration files
  - [ ] No secrets in code repository
  - [ ] Secrets in Key Vault only
  - [ ] Automatic rotation policy configured
  - [ ] Access logging enabled

- [ ] **Monitoring**
  - [ ] Application Insights configured
  - [ ] Log Analytics workspace connected
  - [ ] Alert rules created for critical events
  - [ ] Incident response plan documented
  - [ ] Regular security audits scheduled

---

## References

- [Azure Key Vault Security](https://learn.microsoft.com/azure/key-vault/general/security-overview)
- [Azure SQL Security](https://learn.microsoft.com/azure/azure-sql/database/security-overview)
- [API Management Security](https://learn.microsoft.com/azure/api-management/api-management-security-controls)
- [Managed Identity](https://learn.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)
- [Azure Functions Security](https://learn.microsoft.com/azure/azure-functions/security-concepts)
