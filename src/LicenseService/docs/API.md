# License Service API Documentation

The License Service exposes RESTful APIs for license validation, offline token generation, and Stripe webhook handling. This document describes the available endpoints, request/response formats, and authentication requirements.

## Authentication

All API endpoints (except the Stripe webhook endpoint) require the following header:

```
Ocp-Apim-Subscription-Key: your-license-key
```

In production, this authentication is handled by Azure API Management (APIM).

## API Endpoints

### License Validation

Validates a license key for an application or feature.

**Endpoint:** `POST /api/license/validate`

**Headers:**
- `Ocp-Apim-Subscription-Key`: The license key to validate
- `Content-Type`: application/json

**Request Body:**
```json
{
  "appGuid": "12345678-1234-1234-1234-123456789012",
  "featureId": "premium-feature"  // Optional
}
```

**Response (200 OK - Valid License):**
```json
{
  "isValid": true,
  "status": "active",
  "message": "License is valid",
  "plan": "Pro",
  "features": ["feature1", "feature2"],
  "expirationDate": "2023-12-31T23:59:59Z",
  "trialDaysRemaining": null,
  "paymentDue": false
}
```

**Response (403 Forbidden - Invalid License):**
```json
{
  "isValid": false,
  "status": "expired",
  "message": "License has expired",
  "plan": null,
  "features": [],
  "expirationDate": "2023-01-01T00:00:00Z",
  "trialDaysRemaining": null,
  "paymentDue": false
}
```

**Possible Status Values:**
- `active`: License is active and valid
- `not_found`: License key not found
- `app_guid_mismatch`: The license key is not valid for this application
- `expired`: License has expired
- `trial_expired`: Trial period has expired
- `paused`: License is temporarily paused
- `canceled`: License has been canceled
- `feature_not_entitled`: The requested feature is not included in the license
- `past_due`: Payment is due but license is still valid
- `error`: An error occurred during validation

### Offline Token Generation

Generates an offline token for use when the application will be offline.

**Endpoint:** `POST /api/license/offlineToken`

**Headers:**
- `Ocp-Apim-Subscription-Key`: The license key
- `Content-Type`: application/json

**Request Body:**
```json
{
  "appGuid": "12345678-1234-1234-1234-123456789012",
  "requestedDurationDays": 7,  // Optional
  "deviceId": "device-identifier"  // Optional
}
```

**Response (200 OK - Success):**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expirationDate": "2023-02-07T14:30:00Z",
  "message": "Offline token valid until 2023-02-07 14:30:00"
}
```

**Response (403 Forbidden - Failure):**
```json
{
  "success": false,
  "token": null,
  "expirationDate": null,
  "message": "License key not found"
}
```

### Stripe Webhook

Handles Stripe webhook events to update license status based on subscription changes.

**Endpoint:** `POST /api/stripewebhook`

**Headers:**
- `Stripe-Signature`: Signature provided by Stripe for webhook verification
- `Content-Type`: application/json

**Request Body:**
The request body is a Stripe event object. See [Stripe's webhook documentation](https://stripe.com/docs/webhooks) for details.

**Response (200 OK - Success):**
```json
{
  "received": true
}
```

## Offline Token Format

The offline token is a JWT (JSON Web Token) with the following claims:

- `licenseId`: The license ID
- `appGuid`: The application GUID the token is valid for
- `deviceId`: (Optional) The device ID the token is bound to
- `issuedAt`: When the token was issued (ISO 8601 format)
- `expiresAt`: When the token expires (ISO 8601 format)
- `features`: Array of feature IDs the license is entitled to
- `plan`: The license plan name

## Error Handling

The API uses standard HTTP status codes:

- `200 OK`: Request successful
- `400 Bad Request`: Missing or invalid request parameters
- `403 Forbidden`: Invalid license key or unauthorized access
- `500 Internal Server Error`: Server-side error

Error responses include a message field with a human-readable description of the error.

## Rate Limiting

In production, rate limiting is enforced by Azure API Management. The default limits are:

- 60 requests per minute per subscription key for validation endpoints
- 10 requests per minute per subscription key for offline token generation 