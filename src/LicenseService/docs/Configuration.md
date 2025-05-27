# License Service Configuration Guide

This guide explains how to configure the License Service for different environments.

## Configuration Files

The License Service uses the standard ASP.NET Core configuration system, with the following configuration files:

- `appsettings.json`: Base configuration for all environments
- `appsettings.Development.json`: Development-specific overrides
- `appsettings.Production.json`: Production-specific overrides
- `appsettings.Staging.json`: Staging-specific overrides

## Configuration Sections

### Logging

Configure logging levels and providers:

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft": "Warning",
    "Microsoft.Hosting.Lifetime": "Information"
  }
}
```

### NLog Configuration

The service uses NLog for structured logging. The NLog configuration is in `nlog.config`:

```xml
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true">
  <!-- Configure targets and rules here -->
</nlog>
```

For production, consider configuring NLog to write to Azure Application Insights or another centralized logging system.

### Stripe Integration

Configure Stripe API keys and webhook secrets:

```json
"Stripe": {
  "SecretKey": "sk_test_your_test_key",
  "WebhookSecret": "whsec_your_webhook_signing_secret"
}
```

For production, it's recommended to use environment variables or Azure Key Vault instead of storing secrets in configuration files:

```json
"Stripe": {
  "SecretKey": "#{Stripe:SecretKey}#",
  "WebhookSecret": "#{Stripe:WebhookSecret}#"
}
```

### Offline Token Settings

Configure JWT token settings for offline tokens:

```json
"OfflineToken": {
  "SigningKey": "your_development_signing_key_replace_in_production",
  "Issuer": "LicenseService",
  "Audience": "LicensedApplications"
}
```

For production, generate a strong signing key and store it securely in Azure Key Vault.

### License Settings

Configure license-related settings:

```json
"License": {
  "MaxOfflineDays": 14
}
```

## Environment Variables

For production environments, use environment variables to override configuration values:

- `ASPNETCORE_ENVIRONMENT`: Set to `Development`, `Staging`, or `Production`
- `Stripe__SecretKey`: Stripe API secret key
- `Stripe__WebhookSecret`: Stripe webhook signing secret
- `OfflineToken__SigningKey`: JWT signing key
- `License__MaxOfflineDays`: Maximum allowed offline days

## Azure Key Vault Integration

For secure storage of secrets in production, configure Azure Key Vault integration:

1. Add the Azure Key Vault configuration provider to `Program.cs`:

```csharp
// Add Key Vault configuration provider
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVault:Name"]}.vault.azure.net/"),
    new DefaultAzureCredential());
```

2. Add the required packages:

```bash
dotnet add package Azure.Identity
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
```

3. Configure Key Vault access in Azure:
   - Assign the appropriate managed identity to the service
   - Grant the managed identity "Get" and "List" permissions for secrets

## CORS Configuration

If your client applications need to call the API directly (not through APIM), configure CORS:

```json
"Cors": {
  "AllowedOrigins": [
    "https://app.example.com",
    "https://admin.example.com"
  ]
}
```

And in `Program.cs`:

```csharp
// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy",
        builder => builder
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())
            .AllowAnyMethod()
            .AllowAnyHeader());
});
```

## Health Checks

The service includes a health check endpoint at `/api/health`. Configure additional health checks in `Program.cs`:

```csharp
// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddCheck("stripe", () => 
        {
            try
            {
                var stripeClient = new StripeClient(builder.Configuration["Stripe:SecretKey"]);
                var balanceService = new BalanceService(stripeClient);
                var balance = balanceService.Get();
                return HealthCheckResult.Healthy();
            }
            catch
            {
                return HealthCheckResult.Unhealthy();
            }
        });
``` 