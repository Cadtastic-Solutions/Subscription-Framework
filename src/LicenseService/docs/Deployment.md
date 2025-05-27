# License Service Deployment Guide

This guide outlines the steps to deploy the License Service to Azure.

## Prerequisites

- Azure subscription
- Azure CLI or Azure PowerShell installed
- .NET 8.0 SDK
- Git

## Azure Resources

The following Azure resources are required:

- **Azure App Service**: To host the License Service
- **Azure API Management**: To manage API access and subscription keys
- **Azure Key Vault**: To securely store secrets
- **Azure SQL Database**: For production license data storage
- **Azure Application Insights**: For monitoring and logging

## Deployment Steps

### 1. Prepare the Application

1. Build the application in Release mode:

```bash
dotnet publish -c Release -o ./publish
```

2. Verify the application works locally:

```bash
cd publish
dotnet LicenseService.dll
```

### 2. Create Azure Resources

You can create the required Azure resources using Azure CLI, Azure PowerShell, or the Azure Portal.

Using Azure CLI:

```bash
# Variables
RESOURCE_GROUP="subscription-framework-rg"
LOCATION="eastus"
APP_NAME="license-service"
API_MANAGEMENT="subscription-apim"
KEYVAULT_NAME="subscription-kv"
SQL_SERVER="subscription-sql"
SQL_DB="LicenseDB"
APP_INSIGHTS="subscription-insights"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create App Service Plan
az appservice plan create --name "${APP_NAME}-plan" --resource-group $RESOURCE_GROUP --sku P1V2

# Create App Service
az webapp create --name $APP_NAME --resource-group $RESOURCE_GROUP --plan "${APP_NAME}-plan" --runtime "DOTNET|8.0"

# Create Key Vault
az keyvault create --name $KEYVAULT_NAME --resource-group $RESOURCE_GROUP --location $LOCATION

# Create SQL Server and Database
az sql server create --name $SQL_SERVER --resource-group $RESOURCE_GROUP --location $LOCATION --admin-user "sqladmin" --admin-password "P@ssw0rd1234!"

az sql db create --name $SQL_DB --resource-group $RESOURCE_GROUP --server $SQL_SERVER --service-objective S1

# Create Application Insights
az monitor app-insights component create --app $APP_INSIGHTS --location $LOCATION --resource-group $RESOURCE_GROUP

# Create API Management (if not already existing)
az apim create --name $API_MANAGEMENT --resource-group $RESOURCE_GROUP --publisher-name "Your Company" --publisher-email "admin@example.com" --sku-name Developer
```

### 3. Configure Azure App Service

1. Set the application settings:

```bash
az webapp config appsettings set --name $APP_NAME --resource-group $RESOURCE_GROUP --settings \
  ASPNETCORE_ENVIRONMENT="Production" \
  APPINSIGHTS_INSTRUMENTATIONKEY="$(az monitor app-insights component show --app $APP_INSIGHTS --resource-group $RESOURCE_GROUP --query instrumentationKey -o tsv)" \
  KeyVault__Name=$KEYVAULT_NAME \
  ConnectionStrings__LicenseDatabase="Data Source=$SQL_SERVER.database.windows.net;Initial Catalog=$SQL_DB;User ID=sqladmin;Password=P@ssw0rd1234!"
```

2. Enable managed identity for the App Service:

```bash
# Enable system-assigned managed identity
az webapp identity assign --name $APP_NAME --resource-group $RESOURCE_GROUP

# Get the principal ID
PRINCIPAL_ID=$(az webapp identity show --name $APP_NAME --resource-group $RESOURCE_GROUP --query principalId -o tsv)

# Grant Key Vault access
az keyvault set-policy --name $KEYVAULT_NAME --object-id $PRINCIPAL_ID --secret-permissions get list
```

### 4. Configure Azure Key Vault Secrets

Store your sensitive configuration in Azure Key Vault:

```bash
az keyvault secret set --vault-name $KEYVAULT_NAME --name "Stripe--SecretKey" --value "sk_live_your_stripe_key"
az keyvault secret set --vault-name $KEYVAULT_NAME --name "Stripe--WebhookSecret" --value "whsec_your_webhook_secret"
az keyvault secret set --vault-name $KEYVAULT_NAME --name "OfflineToken--SigningKey" --value "your_production_signing_key"
```

### 5. Deploy the Application

1. Deploy using ZIP deployment:

```bash
# Create a ZIP file
cd publish
zip -r ../license-service.zip .

# Deploy the ZIP file
az webapp deployment source config-zip --name $APP_NAME --resource-group $RESOURCE_GROUP --src ../license-service.zip
```

2. Alternatively, set up continuous deployment from GitHub:

```bash
# Configure GitHub deployment
az webapp deployment source config --name $APP_NAME --resource-group $RESOURCE_GROUP \
  --repo-url "https://github.com/your-org/subscription-framework" \
  --branch main --manual-integration
```

### 6. Configure Azure API Management

1. Import the License Service API:

```bash
# Create an API
az apim api create --service-name $API_MANAGEMENT --resource-group $RESOURCE_GROUP \
  --api-id "license-service" \
  --display-name "License Service API" \
  --path "license" \
  --protocols "https" \
  --service-url "https://$APP_NAME.azurewebsites.net"

# Import API from OpenAPI specification (Swagger)
az apim api import --service-name $API_MANAGEMENT --resource-group $RESOURCE_GROUP \
  --api-id "license-service" \
  --path "license" \
  --specification-format "OpenApiJson" \
  --specification-url "https://$APP_NAME.azurewebsites.net/swagger/v1/swagger.json"
```

2. Set up APIM policies:

   - Configure the API to require subscription keys
   - Set up App GUID validation
   - Configure rate limiting

Example policy:

```xml
<policies>
    <inbound>
        <base />
        <check-header name="X-App-Id" failed-check-httpcode="403" failed-check-error-message="Missing or invalid application ID" ignore-case="false" />
        <rate-limit-by-key calls="60" renewal-period="60" counter-key="@(context.Subscription.Key)" />
    </inbound>
    <backend>
        <base />
    </backend>
    <outbound>
        <base />
    </outbound>
    <on-error>
        <base />
    </on-error>
</policies>
```

### 7. Configure Stripe Webhook

1. In your Stripe dashboard, create a webhook pointing to:

```
https://$API_MANAGEMENT.azure-api.net/license/api/stripewebhook
```

2. Configure the webhook to listen for the following events:
   - `checkout.session.completed`
   - `invoice.payment_succeeded`
   - `invoice.payment_failed`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`

3. Copy the webhook signing secret to Azure Key Vault.

### 8. Verify Deployment

1. Test the health endpoint:

```
https://$APP_NAME.azurewebsites.net/api/health
```

2. Test the API through API Management:

```
curl -X POST "https://$API_MANAGEMENT.azure-api.net/license/api/license/validate" \
  -H "Ocp-Apim-Subscription-Key: your-subscription-key" \
  -H "X-App-Id: your-app-guid" \
  -H "Content-Type: application/json" \
  -d '{"appGuid": "your-app-guid"}'
```

## Monitoring and Alerts

1. Set up Application Insights alerts for:
   - High failure rates
   - Increased response times
   - Exceptions

2. Configure diagnostic settings to send logs to:
   - Log Analytics workspace
   - Azure Storage Account for long-term retention

## Backup and Disaster Recovery

1. Configure SQL Database backups
2. Set up geo-replication for the SQL Database
3. Document the recovery process in case of service outage 