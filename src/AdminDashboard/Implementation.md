# AdminDashboard Implementation

This walkthrough explains how to build the admin portal using ASP.NET Core Razor Pages. The steps mirror the architecture described in the repository root.

## 1. Project Scaffold
```bash
dotnet new webapp -n AdminDashboard
```
This creates a Razor Pages project targeted to .NET 6.

## 2. Add Shared Library
Reference the `Shared` project for common models:
```bash
dotnet add reference ../Shared/Shared.csproj
```

## 3. Configure Authentication
Use Azure AD for internal sign-in:
```bash
dotnet add package Microsoft.AspNetCore.Authentication.AzureAD.UI
```
Update `Program.cs` to enable Azure AD authentication and restrict access to authorized users.

## 4. Integrate APIM and License Service
Implement pages that call the License Service APIs and Azure API Management management API to manage products and keys. Store secrets such as APIM credentials and Stripe keys in Azure Key Vault and load them at startup.

## 5. Running and Debugging
```bash
dotnet run
```
The dashboard runs at `https://localhost:5001`. Update `appsettings.Development.json` with test configuration.
