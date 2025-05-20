param location string = resourceGroup().location
param namePrefix string = 'sf'
param skuName string = 'B1'

var storageAccountName = uniqueString(resourceGroup().id, namePrefix)

resource storage 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: '${namePrefix}-plan'
  location: location
  sku: {
    name: skuName
    tier: 'Basic'
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${namePrefix}-ai'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: '${namePrefix}-func'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storage.properties.primaryEndpoints.blob
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
      ]
    }
  }
  dependsOn: [storage, appServicePlan, appInsights]
}

resource apim 'Microsoft.ApiManagement/service@2022-08-01' = {
  name: '${namePrefix}-apim'
  location: location
  sku: {
    name: 'Consumption'
    capacity: 0
  }
  properties: {
    publisherEmail: 'admin@example.com'
    publisherName: 'Example Publisher'
  }
}

output functionAppEndpoint string = functionApp.properties.defaultHostName

