# License Service Documentation

Welcome to the License Service documentation. This service is part of the Subscription Framework and handles license issuance, validation, and Stripe subscription integration.

## Documentation Contents

- [API Documentation](API.md): Details on the API endpoints, request/response formats, and authentication
- [Configuration Guide](Configuration.md): How to configure the service for different environments
- [Deployment Guide](Deployment.md): Instructions for deploying the service to Azure
- [Development Guide](Development.md): Information for developers working on the service
- [Testing Guide](../tests/LicenseService.Tests/README.md): Information about the test suite and how to run tests

## Quick Start

1. Clone the repository
2. Configure your environment variables or update appsettings.json
3. Run `dotnet run` to start the service locally
4. Access the Swagger UI at https://localhost:5001/swagger

## Architecture Overview

The License Service is a critical component of the Subscription Framework, providing the following functionality:

- License key validation with App GUID enforcement
- Offline license token generation using JWT tokens
- Integration with Stripe for subscription lifecycle management
- Secure token generation and verification

For a detailed architecture overview, see the [Subscription Framework Architecture](../../README.md).
