@description('Name of the Logic App workflow to create')
param logicAppName string = 'order-notification-la'

@description('Location for all resources')
param location string = resourceGroup().location

@description('API connection resource name for SQL')
param sqlConnectionName string = 'sql-connection-orderintegrationpoc'

@description('SQL Server fully qualified host name or server name')
param sqlServer string

@description('SQL Database name')
param sqlDatabase string = 'OrderIntegrationPOC_DB'

@description('SQL login username (SQL Authentication)')
param sqlUsername string

@secure()
@description('SQL login password (SQL Authentication)')
param sqlPassword string

@description('API connection resource name for Office 365 Outlook')
param officeConnectionName string = 'office365-connection-orderintegrationpoc'

@description('API connection resource name for Microsoft Teams')
param teamsConnectionName string = 'teams-connection-orderintegrationpoc'

@description('Recipient email for notifications')
param recipientEmail string

@description('Target Teams team id (GUID) for posting Adaptive Card')
param teamId string

@description('Target Teams channel id (GUID) for posting Adaptive Card')
param channelId string

// Managed API resource IDs (location-scoped)
var sqlApiId = subscriptionResourceId('Microsoft.Web', 'locations', location, 'managedApis/sql')
var officeApiId = subscriptionResourceId('Microsoft.Web', 'locations', location, 'managedApis/office365')
var teamsApiId = subscriptionResourceId('Microsoft.Web', 'locations', location, 'managedApis/teams')

// API Connection: SQL
resource sqlConnection 'Microsoft.Web/connections@2016-06-01' = {
  name: sqlConnectionName
  location: location
  properties: {
	displayName: sqlConnectionName
	parameterValues: {
	  server: sqlServer
	  database: sqlDatabase
	  authenticationType: 'SQLAuthentication'
	  username: sqlUsername
	  password: sqlPassword
	}
	api: {
	  id: sqlApiId
	}
  }
}

// API Connection: Office 365 (requires authorization in portal after deployment)
resource officeConnection 'Microsoft.Web/connections@2016-06-01' = {
  name: officeConnectionName
  location: location
  properties: {
	displayName: officeConnectionName
	// No secret values here; the connection typically requires interactive OAuth consent after deployment
	parameterValues: {}
	api: {
	  id: officeApiId
	}
  }
}

// API Connection: Microsoft Teams (requires authorization in portal after deployment)
resource teamsConnection 'Microsoft.Web/connections@2016-06-01' = {
  name: teamsConnectionName
  location: location
  properties: {
	displayName: teamsConnectionName
	parameterValues: {}
	api: {
	  id: teamsApiId
	}
  }
}

// Logic App workflow
resource logicApp 'Microsoft.Logic/workflows@2019-05-01' = {
  name: logicAppName
  location: location
  properties: {
	// Load workflow definition from local file shipped with repository
	definition: json(loadTextContent('order-notification.json'))
	parameters: {
	  SqlConnection: {
		value: sqlConnection.id
	  }
	  Office365Connection: {
		value: officeConnection.id
	  }
	  TeamsConnection: {
		value: teamsConnection.id
	  }
	  RecipientEmail: {
		value: recipientEmail
	  }
	  TeamId: {
		value: teamId
	  }
	  ChannelId: {
		value: channelId
	  }
	}
	// Enable the workflow by default
	state: 'Enabled'
  }
  dependsOn: [sqlConnection, officeConnection, teamsConnection]
}

output logicAppResourceId string = logicApp.id
output sqlConnectionResourceId string = sqlConnection.id
output officeConnectionResourceId string = officeConnection.id
output teamsConnectionResourceId string = teamsConnection.id
