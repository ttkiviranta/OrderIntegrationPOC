@description('Project name for resource naming')
param projectName string = 'orderintegrationpoc'

@description('Environment name (dev, staging, prod)')
param environment string = 'dev'

@description('Location for all resources')
param location string = resourceGroup().location

@description('Key Vault Name')
param keyVaultName string = '${projectName}-kv-${environment}'

@description('API Management Name')
param apiManagementName string = '${projectName}-apim-${environment}'

@description('Publisher name for APIM')
param apimPublisherName string = 'OrderIntegration'

@description('Publisher email for APIM')
param apimPublisherEmail string

@description('Tags to apply to all resources')
param tags object = {
  project: projectName
  environment: environment
  deployment: utcNow('u')
}

// Deploy Key Vault
module keyVaultModule 'key-vault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
	keyVaultName: keyVaultName
	location: location
	tags: tags
  }
}

// Deploy API Management (conditional)
module apiManagementModule 'apim.bicep' = {
  name: 'apiManagementDeployment'
  params: {
	apiManagementName: apiManagementName
	location: location
	publisherName: apimPublisherName
	publisherEmail: apimPublisherEmail
	tags: tags
  }
}

@export()
output keyVaultName string = keyVaultModule.outputs.keyVaultName
@export()
output keyVaultUri string = keyVaultModule.outputs.keyVaultUri
@export()
output apiManagementName string = apiManagementModule.outputs.apiManagementName
@export()
output apimanagementGatewayUrl string = apiManagementModule.outputs.gatewayUrl
@export()
output deploymentTimestamp string = utcNow('u')
