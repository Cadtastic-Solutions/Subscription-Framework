// Bicep template for Subscription Framework
// Deploys resources needed for Stripe webhook handling and APIM integration

param location string = resourceGroup().location
param storageAccountName string
param functionAppName string
param appServicePlanName string
param apimServiceName string

resource storage 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

resource plan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: plan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storage.properties.primaryEndpoints.blob
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
    }
  }
}

resource apim 'Microsoft.ApiManagement/service@2022-08-01' existing = {
  name: apimServiceName
}

resource webhook 'Microsoft.ApiManagement/service/apis@2022-08-01' = {
  parent: apim
  name: 'stripe-webhook'
  properties: {
    displayName: 'Stripe Webhook API'
    path: 'stripe/webhook'
    protocols: [ 'https' ]
    subscriptionRequired: false
    isCurrent: true
  }
}

output functionAppUrl string = functionApp.properties.defaultHostName
