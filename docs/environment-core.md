# Environment & Core Service Setup

This guide explains how to set up the initial Azure environment and the skeleton License Service. It follows the steps outlined in [Project Planning](planning.md) for **Environment & Core Service**.

## Prerequisites

- An [Azure](https://azure.microsoft.com/) subscription with permissions to create resources.
- The [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) with [Bicep](https://learn.microsoft.com/azure/azure-resource-manager/bicep/install) support.
- [.NET SDK 8.0](https://learn.microsoft.com/dotnet/core/install) for building the License Service.

## Deploying the Environment

The `infra/main.bicep` file provisions core resources:

- **Storage Account** – required for Function App runtime storage.
- **App Service Plan** – a consumption plan for the Function App.
- **Function App** – hosts the License Service.
- **API Management** – exposes API endpoints and manages subscription keys.

### 1. Configure parameters

Edit `infra/main.bicep` to set values like the environment name and publisher email. Parameters can also be supplied on deployment.

### 2. Deploy with Azure CLI

```bash
az deployment sub create \
  --location <region> \
  --template-file infra/main.bicep \
  --parameters environmentName=myenv publisherEmail=admin@example.com
```

The CLI compiles the Bicep file and creates the resources in your subscription. See [Deploy resources with Bicep](https://learn.microsoft.com/azure/azure-resource-manager/bicep/deploy-to-azure) for more deployment options.

## Building the License Service

The License Service is a minimal ASP.NET Core API located in `src/LicenseService`.

### Restore and build

```bash
cd src/LicenseService
 dotnet restore
 dotnet build
```

The service exposes a simple health endpoint at `/api/health`. You can run it locally with `dotnet run` or deploy it to the Function App created earlier. Refer to [Deploy a .NET Function App](https://learn.microsoft.com/azure/azure-functions/functions-develop-vs-code?tabs=csharp) for detailed deployment steps.

## Next Steps

Once the environment is deployed and the service builds successfully, continue with Stripe integration and additional license logic as described in the planning document.
