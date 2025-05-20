# Azure Setup How-To

This guide describes how to provision Azure resources for the Admin Dashboard using the provided Bicep template.

## Prerequisites

- Azure CLI with Bicep installed
- An Azure subscription with permission to create resources

## Steps

1. Clone the repository and navigate to the `infra` folder:
   ```bash
   git clone https://github.com/Cadtastic-Solutions/Subscription-Framework.git
   cd Subscription-Framework/infra
   ```
2. Deploy the template (replace `myResourceGroup` and `myLocation` as needed):
   ```bash
   az group create -n myResourceGroup -l myLocation
   az deployment group create -g myResourceGroup -f adminDashboard.bicep \
     -p webAppName=my-admin-dashboard
   ```

The template creates an App Service plan, a Web App, and Application Insights.

For more details see [Azure Bicep documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/).
