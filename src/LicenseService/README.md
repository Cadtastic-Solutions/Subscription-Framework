# License Service

This service is part of the Subscription Framework and handles license issuance, validation, and Stripe subscription integration.

## Documentation

For detailed documentation, please refer to:

- [API Documentation](docs/API.md) - Details on the API endpoints
- [Configuration Guide](docs/Configuration.md) - How to configure the service
- [Deployment Guide](docs/Deployment.md) - How to deploy to Azure
- [Development Guide](docs/Development.md) - Guide for developers
- [Testing Guide](tests/LicenseService.Tests/README.md) - Information about tests

## Features

- License validation with App GUID enforcement
- Offline license token generation with JWT-based signatures
- Stripe subscription lifecycle integration (trials, payments, cancellations)
- RESTful API for client applications

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Azure API Management instance (for production)
- Stripe account with API keys

### Configuration

1. Update `appsettings.json` with your Stripe API keys and webhook secret
2. For production, use Azure Key Vault for storing secrets

### Running Locally

```bash
# Restore packages
dotnet restore

# Run the service
dotnet run
```

The service will be available at:
- https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

## API Endpoints

- `POST /api/license/validate` - Validate a license key
- `POST /api/license/offlineToken` - Generate an offline token
- `POST /api/stripewebhook` - Handle Stripe webhook events

## License Key Headers

When calling the API, include:
- `Ocp-Apim-Subscription-Key`: The license key (APIM subscription key)
- `X-App-Id`: The application GUID

## Deployment

For production deployment, use Azure App Service and configure the following:

1. Connection to Azure API Management
2. Stripe webhook endpoint URL
3. Secure key storage via Azure Key Vault

See the [Deployment Guide](docs/Deployment.md) for detailed instructions.
