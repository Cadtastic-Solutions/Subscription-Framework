# AdminDashboard

Internal web portal built with **ASP.NET Core** for administrators to manage customers, license plans and Stripe billing status. The dashboard communicates with the License Service and Azure API Management as described in the [Software Licensing Framework - Architecture Overview](../../README.md).

## Purpose
- Create and update APIM products that represent license plans
- View users and their subscriptions
- Trigger license key revocation or regeneration
- Adjust offline grace periods and other license settings

## Running Locally
1. Install the .NET 6 SDK.
2. Navigate to this folder and run:
   ```bash
   dotnet run
   ```
3. The app listens on `https://localhost:5001`.

Refer to the [Admin Dashboard documentation](../../docs/admin-dashboard/user-manual.md) for feature details.
