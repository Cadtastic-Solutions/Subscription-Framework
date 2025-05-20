# ClientSdk Implementation

This guide describes how to build the .NET client library that interacts with the License Service.

## 1. Create the Library
```bash
dotnet new classlib -n ClientSdk
```
Target .NET Standard to allow usage from different application types.

## 2. Add Dependencies
```bash
dotnet add package System.Net.Http.Json
```
Include `Microsoft.IdentityModel.Tokens` if validating JWT offline tokens.

## 3. Implement LicenseClient
Create a class that wraps `HttpClient` and calls the License Service through Azure API Management.
- `ValidateAsync(key, appId)` sends a POST to `/validate`.
- `RequestOfflineTokenAsync(key)` obtains a signed token.
- `VerifyOfflineToken(token, publicKey)` uses RSA/ECDSA to verify the signature.

## 4. Publish
Publish the library as a NuGet package so applications can reference it.
