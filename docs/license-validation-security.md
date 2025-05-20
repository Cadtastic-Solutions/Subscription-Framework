# License Validation & Security Guide

This guide describes how to implement secure license validation for the Subscription Framework. It covers online and offline flows, protecting signing keys with Azure Key Vault, and how to deploy core components.

## Overview

The License Service validates subscription status and enforces application GUID checks. Licenses are represented as API Management subscription keys. The service also issues signed offline tokens for scenarios where the client application cannot reach the service for a limited time.

The implementation references:

- [Azure API Management documentation](https://learn.microsoft.com/azure/api-management/)
- [Azure Key Vault documentation](https://learn.microsoft.com/azure/key-vault/)
- [Azure Functions documentation](https://learn.microsoft.com/azure/azure-functions/)
- [Stripe webhooks documentation](https://stripe.com/docs/webhooks)

## Online Validation Flow

1. Client calls the `/validate` endpoint through API Management using the subscription key and `X-App-Id` header.
2. API Management validates the key and forwards the request to the License Service.
3. The service checks subscription state from the database and ensures the `X-App-Id` matches the stored application GUID.
4. A JSON response is returned indicating whether the license is valid.

## Offline Token Flow

1. Client requests `/offlineToken` to obtain a short-lived token before going offline.
2. The License Service verifies the license, then issues a signed JWT containing the license ID, allowed features, issue time, and expiry.
3. The token is signed using an asymmetric key stored in Azure Key Vault. The public key is embedded in the client application to verify the signature.
4. While offline, the client validates the token locally until it expires, at which point it must contact the service again.

## How To: Request an Offline Token

1. Ensure the user has an active subscription.
2. Make a POST request to `/offlineToken` with the subscription key and `X-App-Id` header.
3. The service responds with a JWT similar to:

```json
{
  "token": "<signed JWT>",
  "expires": "2025-06-30T00:00:00Z"
}
```

4. Store this token securely on the client device. When offline, verify the token signature and expiry before enabling features.

## Securing Signing Keys

1. Create an Azure Key Vault and generate an RSA key pair.
2. Grant the License Service access to sign tokens with the private key.
3. Export the corresponding public key and bundle it with the client application.
4. Rotate keys periodically using Key Vault key versions.

## Deployment Template

The `azure/license-infrastructure.bicep` file provisions the required Azure resources:

- Azure Function App hosting the License Service
- Azure SQL Database for license data
- Azure Key Vault storing the signing key
- API Management instance with a product representing the license plan

See the [deployment template](../azure/license-infrastructure.bicep) for details.

## Sample License Service

A minimal Python implementation is provided in `src/license_service/main.py`. It uses built-in modules and HMAC signing for tokens. Run the service locally with:

```bash
python3 src/license_service/main.py
```

Use `python3 -m py_compile src/license_service/main.py` to check for syntax errors.

