# LicenseService Tests

This project contains unit tests for the License Service API.

## Test Categories

Tests are organized into categories to make it easier to run specific types of tests. The following categories are available:

### Component Categories
- **Controller**: Tests for API controllers
- **Repository**: Tests for data access
- **LicenseService**: Tests for the main license service implementation
- **Crypto**: Tests for cryptographic operations

### Feature Categories
- **LicenseValidation**: Tests for license key validation
- **OfflineToken**: Tests for offline token generation and validation
- **TokenSigning**: Tests specifically for signing JWT tokens
- **TokenVerification**: Tests specifically for verifying JWT tokens
- **StripeWebhook**: Tests for Stripe webhook processing
- **FeatureEntitlement**: Tests for feature-level entitlement checks

### Cross-cutting Categories
- **Security**: Tests related to security features (signature verification, app GUID enforcement)
- **ErrorHandling**: Tests for error conditions and edge cases
- **ExceptionHandling**: Tests for exception scenarios
- **Integration**: Tests that verify integration with external services
- **Create/Read/Update/Delete**: CRUD operation tests for the repository
- **Configuration**: Tests for configuration-related logic
- **Expiration**: Tests for handling expired licenses

## Running Tests by Category

To run tests from a specific category, use the `--filter` option with `dotnet test`:

```bash
# Run all controller tests
dotnet test --filter "TestCategory=Controller"

# Run all security-related tests
dotnet test --filter "TestCategory=Security"

# Run tests that are both related to controllers and error handling
dotnet test --filter "TestCategory=Controller & TestCategory=ErrorHandling"

# Run tests related to validation OR token verification
dotnet test --filter "TestCategory=Validation | TestCategory=TokenVerification"
```

## Test Naming Convention

Tests follow the naming convention of:

```
MethodName_Scenario_ExpectedBehavior
```

For example:
- `ValidateLicenseAsync_WhenLicenseExpired_ShouldReturnInvalidResponse`
- `VerifyTokenSignatureAsync_WithTamperedToken_ShouldReturnFalse` 