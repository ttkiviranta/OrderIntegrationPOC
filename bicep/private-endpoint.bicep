@description('Name of the private endpoint resource')
param privateEndpointName string

@description('Name of the resource that this private endpoint connects to (SQL Server, Storage Account, etc.)')
param resourceName string

@description('Resource Group where the resource to connect to is located')
param resourceGroupName string

@description('Type of service (sql, blob, queue, etc.)')
param serviceType string

@description('Resource ID of the virtual network where private endpoint will be created')
param virtualNetworkId string

@description('Name of the subnet within the virtual network')
param subnetName string

@description('Location for the private endpoint')
param location string = resourceGroup().location

@description('Tags to apply to the private endpoint')
param tags object = {}

// Reference to the subnet
resource subnet 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' existing = {
  name: '${last(split(virtualNetworkId, '/'))}/subnets/${subnetName}'
  scope: resourceGroup(resourceGroupName)
}

// Get the resource ID of the resource to connect to
var resourceId = resourceId(resourceGroupName, 'Microsoft.Sql/servers', resourceName)

// Create private endpoint
resource privateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpointName
  location: location
  tags: tags
  properties: {
	subnet: {
	  id: subnet.id
	}
	privateLinkServiceConnections: [
	  {
		name: '${resourceName}-connection'
		properties: {
		  privateLinkServiceId: resourceId
		  groupIds: [
			serviceType
		  ]
		}
	  }
	]
  }
}

// Create private DNS zone group for SQL (example)
resource privateDnsZoneGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = if (serviceType == 'sqlServer') {
  parent: privateEndpoint
  name: 'default'
  properties: {
	privateDnsZoneConfigs: [
	  {
		name: 'privatelink-database-windows-net'
		properties: {
		  privateDnsZoneId: resourceId('Microsoft.Network/privateDnsZones', 'privatelink.database.windows.net')
		}
	  }
	]
  }
}

@export()
output privateEndpointId string = privateEndpoint.id
@export()
output privateEndpointName string = privateEndpoint.name
@export()
output networkInterfaceIds array = privateEndpoint.properties.networkInterfaces
