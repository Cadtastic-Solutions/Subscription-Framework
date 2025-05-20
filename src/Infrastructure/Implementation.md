# Infrastructure Implementation

Follow these steps to provision the Azure resources required for the framework.

## 1. Install Tools
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- Bicep CLI or Terraform

## 2. Deploy with Bicep
```bash
az deployment sub create \
  --location <region> \
  --template-file main.bicep \
  --parameters environment=dev
```
The template sets up API Management, an App Service for the License Service, Azure SQL or Cosmos DB, and Key Vault with secrets for Stripe and signing keys.

## 3. Deploy with Terraform (optional)
```bash
terraform init
terraform apply -var="environment=dev"
```
Choose one IaC approach and keep state files secure.

## 4. Post Deployment
After resources are created, update the Admin Dashboard and License Service configuration to point to the new endpoints and Key Vault.
