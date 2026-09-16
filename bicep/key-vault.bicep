@description('Creates Key Vault for storing application secrets')
@export()
param keyVaultName string

@description('Location for the Key Vault resource')
param location string = resourceGroup().location

@description('Tags to apply to the Key Vault')
param tags object = {}

// Create Key Vault with proper security settings
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
	enabledForDeployment: false
	enabledForTemplateDeployment: true
	enabledForDiskEncryption: false
	tenantId: subscription().tenantId
	sku: {
	  family: 'A'
	  name: 'standard'
	}
	accessPolicies: []
	networkAcls: {
	  defaultAction: 'Deny'
	  bypass: 'AzureServices'
	}
  }
}

// Secret for SQL connection string
resource sqlConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'SqlConnectionString'
  properties: {
	value: ''  // Provide value during deployment
  }
}

// Secret for Service Bus connection string
resource serviceBusConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ServiceBusConnection'
  properties: {
	value: ''  // Provide value during deployment
  }
}

// Secret for Application Insights connection string
resource appInsightsConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ApplicationInsightsConnectionString'
  properties: {
	value: ''  // Provide value during deployment
  }
}

@export()
output keyVaultId string = keyVault.id
@export()
output keyVaultUri string = keyVault.properties.vaultUri
@export()
output keyVaultName string = keyVault.name
