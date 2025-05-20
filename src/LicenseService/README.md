# LicenseService

ASP.NET Core Web API that implements the core licensing logic. It validates license keys issued through Azure API Management, generates signed offline tokens and processes Stripe webhook events. For architectural context see the [Architecture Overview](../../README.md).

## Responsibilities
- Verify incoming requests via APIM subscription keys and App GUID
- Generate cryptographically signed offline tokens using keys stored in Azure Key Vault
- Update license state based on Stripe webhook notifications
- Expose admin endpoints consumed by the Admin Dashboard
