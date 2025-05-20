# LicenseService Implementation

This section outlines how to build the ASP.NET Core API that drives license issuance and validation.

## 1. Create the Project
```bash
dotnet new webapi -n LicenseService
```

## 2. Add Packages
```bash
dotnet add package Microsoft.Azure.Management.ApiManagement
dotnet add package Stripe.net
```
Configure `appsettings.json` to load configuration from environment variables or Azure Key Vault.

## 3. Implement Endpoints
- `/validate` verifies a license key and App GUID via APIM.
- `/offlineToken` issues a signed offline token using a private key from Key Vault.
- `/stripe-webhook` handles Stripe events to update license status.

## 4. Run Locally
```bash
dotnet run
```
The service runs on `https://localhost:5001` and should be invoked through APIM for production scenarios.
