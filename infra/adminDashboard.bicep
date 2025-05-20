param webAppName string
param location string = resourceGroup().location

resource plan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: '${webAppName}-plan'
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
}

resource app 'Microsoft.Web/sites@2022-09-01' = {
  name: webAppName
  location: location
  properties: {
    serverFarmId: plan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|6.0'
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${webAppName}-insights'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

output webAppUrl string = 'https://${app.properties.defaultHostName}'
