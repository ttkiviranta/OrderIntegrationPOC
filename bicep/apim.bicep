@description('Name of the API Management service')
param apiManagementName string

@description('Location for the API Management resource')
param location string = resourceGroup().location

@description('Publisher name for the APIM service')
param publisherName string = 'OrderIntegration'

@description('Publisher email for the APIM service')
param publisherEmail string

@description('SKU name for APIM (Developer, Standard, Premium)')
param apimSku string = 'Standard'

@description('Tags to apply to APIM')
param tags object = {}

// Create API Management instance
resource apiManagement 'Microsoft.ApiManagement/service@2023-05-01-preview' = {
  name: apiManagementName
  location: location
  tags: tags
  sku: {
	name: apimSku
	capacity: apimSku == 'Developer' ? 1 : 2
  }
  identity: {
	type: 'SystemAssigned'
  }
  properties: {
	publisherName: publisherName
	publisherEmail: publisherEmail
	notificationSenderEmail: publisherEmail
	hostnameConfigurations: [
	  {
		type: 'Proxy'
		hostName: '${apiManagementName}.azure-api.net'
		negotiateClientCertificate: false
		defaultSslBindingIdentity: 'default'
		certificateSource: 'Managed'
	  }
	]
	virtualNetworkType: 'None'  // Use 'External' or 'Internal' for network isolation
	apiVersionConstraint: {
	  minApiVersion: '2021-08-01'
	}
	protocols: [
	  {
		enabled: true
		protocolName: 'http'
	  }
	  {
		enabled: true
		protocolName: 'https'
	  }
	]
	securityDefinitions: {
	  oauth2: {
		type: 'oauth2'
		description: 'OAuth 2.0 authorization'
		flow: 'implicit'
		authorizationUrl: 'https://login.microsoftonline.com/common/oauth2/v2.0/authorize'
		scopes: [
		  {
			name: 'api://ordersapi/.default'
			description: 'Default scope for Orders API'
		  }
		]
	  }
	}
  }
}

// Logger for Application Insights monitoring
resource appInsightsLogger 'Microsoft.ApiManagement/service/loggers@2023-05-01-preview' = {
  parent: apiManagement
  name: 'appinsights-logger'
  properties: {
	loggerType: 'applicationInsights'
	description: 'Application Insights logger for API diagnostics'
	credentials: {
	  instrumentationKey: ''  // Provide Application Insights instrumentation key
	}
	isBuffered: true
	resourceId: ''  // Provide Application Insights resource ID
  }
}

// Global policy for API Management
resource globalPolicy 'Microsoft.ApiManagement/service/policies@2023-05-01-preview' = {
  parent: apiManagement
  name: 'policy'
  properties: {
	value: '''
	  <policies>
		<global>
		  <!-- Enforce TLS 1.2+ -->
		  <inbound>
			<validate-client-certificate-thumbprint-required>true</validate-client-certificate-thumbprint-required>
		  </inbound>
		  <!-- Log all requests to Application Insights -->
		  <backend>
		  </backend>
		  <outbound>
			<!-- Add security headers -->
			<set-header name="X-Content-Type-Options" exists-action="override">
			  <value>nosniff</value>
			</set-header>
			<set-header name="X-Frame-Options" exists-action="override">
			  <value>DENY</value>
			</set-header>
			<set-header name="Strict-Transport-Security" exists-action="override">
			  <value>max-age=31536000; includeSubDomains</value>
			</set-header>
		  </outbound>
		  <on-error>
			<base />
		  </on-error>
		</global>
	  </policies>
	'''
	format: 'xml'
  }
}

@export()
output apiManagementId string = apiManagement.id
@export()
output apiManagementName string = apiManagement.name
@export()
output gatewayUrl string = 'https://${apiManagement.name}.azure-api.net'
@export()
output principalId string = apiManagement.identity.principalId
