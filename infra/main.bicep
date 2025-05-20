param environmentName string = 'licensing'
param location string = resourceGroup().location
param publisherEmail string

resource storage 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: toLower('${environmentName}sa')
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource plan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: '${environmentName}-plan'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: '${environmentName}-func'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storage.properties.primaryEndpoints.blob
        }
      ]
    }
  }
}

resource apim 'Microsoft.ApiManagement/service@2022-08-01' = {
  name: '${environmentName}-apim'
  location: location
  sku: {
    name: 'Consumption'
    capacity: 0
  }
  properties: {
    publisherEmail: publisherEmail
    publisherName: 'Cadtastic Solutions'
  }
}

output functionUrl string = 'https://' + functionApp.properties.defaultHostName
