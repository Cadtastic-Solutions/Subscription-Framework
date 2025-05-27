# License Service Development Guide

This guide provides information for developers working on the License Service.

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or Visual Studio Code
- Git
- Docker (optional, for containerized development)
- Stripe CLI (optional, for local webhook testing)

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/your-org/subscription-framework.git
cd subscription-framework/src/LicenseService
```

### Restore Dependencies

```bash
dotnet restore
```

### Local Development

1. Update your `appsettings.Development.json` with your development settings:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Stripe": {
    "SecretKey": "sk_test_your_test_key",
    "WebhookSecret": "whsec_your_webhook_signing_secret"
  },
  "OfflineToken": {
    "SigningKey": "your_development_signing_key",
    "Issuer": "LicenseService",
    "Audience": "LicensedApplications"
  },
  "License": {
    "MaxOfflineDays": 14
  }
}
```

2. Run the application:

```bash
dotnet run --launch-profile LicenseService
```

3. Access the Swagger UI at https://localhost:5001/swagger

### Database Development

The License Service uses Entity Framework Core for data access. For local development, an in-memory repository is used. 

If you need to work with a database:

1. Install the Entity Framework Core tools:

```bash
dotnet tool install --global dotnet-ef
```

2. Add a SQL Server implementation of the repository:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

3. Create migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Project Structure

- `Controllers/`: API controllers
- `Interfaces/`: Service and repository interfaces
- `Models/`: Data models
- `Services/`: Service implementations
- `docs/`: Documentation
- `tests/`: Unit tests

## Adding New Features

When adding new features, follow these guidelines:

1. **Interface-First Development**: Define interfaces before implementations
2. **Testing**: Write unit tests for all new functionality
3. **Documentation**: Update API documentation when adding or changing endpoints
4. **Validation**: Add proper validation for all input
5. **Error Handling**: Use consistent error handling patterns

### Example: Adding a New Endpoint

1. Create or update the appropriate model class:

```csharp
public class NewFeatureRequest
{
    public string Parameter1 { get; set; } = string.Empty;
    public int Parameter2 { get; set; }
}

public class NewFeatureResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

2. Add the method to the service interface:

```csharp
public interface ILicenseService
{
    // Existing methods...
    
    Task<NewFeatureResponse> NewFeatureAsync(string subscriptionKey, NewFeatureRequest request);
}
```

3. Implement the method in the service:

```csharp
public async Task<NewFeatureResponse> NewFeatureAsync(string subscriptionKey, NewFeatureRequest request)
{
    // Implementation
}
```

4. Add the endpoint to the controller:

```csharp
[HttpPost("newfeature")]
public async Task<ActionResult<NewFeatureResponse>> NewFeature([FromBody] NewFeatureRequest request)
{
    // Implementation
}
```

5. Write unit tests:

```csharp
[TestMethod]
[TestCategory("LicenseService")]
[TestCategory("NewFeature")]
public async Task NewFeatureAsync_WhenParametersValid_ShouldReturnSuccess()
{
    // Test implementation
}
```

6. Update the API documentation in `docs/API.md`

## Testing

### Unit Tests

Run the unit tests:

```bash
dotnet test
```

Run specific test categories:

```bash
dotnet test --filter "TestCategory=LicenseValidation"
```

See the [Testing Guide](../tests/LicenseService.Tests/README.md) for more details.

### Integration Testing

For integration testing with Stripe webhooks, use the Stripe CLI:

```bash
# Install Stripe CLI (see https://stripe.com/docs/stripe-cli)

# Login to Stripe
stripe login

# Forward webhook events to your local server
stripe listen --forward-to https://localhost:5001/api/stripewebhook

# Trigger test events
stripe trigger checkout.session.completed
```

## Debugging

### Visual Studio

1. Open the solution file in Visual Studio
2. Set a breakpoint
3. Press F5 to start debugging

### Visual Studio Code

1. Open the project folder in VS Code
2. Install the C# extension
3. Set a breakpoint
4. Press F5 to start debugging

### Logging

The service uses NLog for logging. Logs are written to the `logs` directory by default.

To add logging to your code:

```csharp
_logger.LogInformation("Processing request for {SubscriptionKey}", subscriptionKey);
_logger.LogWarning("Validation failed: {Reason}", reason);
_logger.LogError(exception, "An error occurred while processing {SubscriptionKey}", subscriptionKey);
```

## Coding Standards

- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use nullable reference types (enabled by default in .NET 8)
- Include XML documentation comments on all public APIs
- Use async/await consistently
- Keep methods small and focused
- Use dependency injection

## Pull Request Process

1. Create a feature branch from `main`
2. Make your changes
3. Ensure all tests pass
4. Update documentation
5. Submit a pull request
6. Get code review from at least one team member
7. Merge after approval

## Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core)
- [Stripe API Documentation](https://stripe.com/docs/api)
- [JWT Documentation](https://jwt.io/introduction)
- [Azure API Management Documentation](https://docs.microsoft.com/en-us/azure/api-management/) 