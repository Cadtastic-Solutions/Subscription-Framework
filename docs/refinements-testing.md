# Refinements & Testing

This guide covers the fifth phase of the project lifecycle where the system is instrumented, monitored and made ready for release. The focus is on **observability**, **quality assurance**, and final **deployment preparations**.

## Monitoring and Analytics

1. **Application Insights** – Deploy an Application Insights instance and configure the License Service to send telemetry. See Microsoft's [official documentation](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview) for detailed guidance.
2. **Log Analytics Workspace** – Centralize logs from the Function App and API Management. Create alerts for errors and unusual activity.
3. **Dashboarding** – Use Azure dashboards or Power BI to visualize key metrics such as active licenses, failed validations and Stripe payment events.

## Testing Strategy

- **Unit Tests** – Implement tests for business logic (e.g. subscription state transitions). When using .NET, frameworks such as `xUnit` are recommended. Run tests locally with:
  ```bash
  dotnet test
  ```
- **Integration Tests** – Validate end‑to‑end flows against a test environment. Tools like `Playwright` or `Postman` collections can automate API calls.
- **Load Testing** – Azure Load Testing can simulate concurrent clients against the API Management gateway.

## Manual Validation Checklist

1. Verify license issuance after a successful Stripe purchase.
2. Confirm that paused or canceled subscriptions block license validation.
3. Review offline token expiry logic.
4. Ensure admin actions are logged and audited.

## Preparing for Deployment

1. **Infrastructure Templates** – Use the provided Bicep template in [`infra/main.bicep`](../infra/main.bicep) to provision Azure resources. Deploy it with the helper script:
   ```bash
   ./infra/deploy.sh my-resource-group eastus
   ```
2. **CI/CD Pipelines** – Configure pipelines (GitHub Actions or Azure DevOps) to run tests and deploy the Bicep template automatically on successful builds.
3. **Documentation** – Finalize user manuals and API references so internal teams can support the system.

## Further Reading

- [Azure API Management Documentation](https://learn.microsoft.com/azure/api-management/)
- [Azure Functions Documentation](https://learn.microsoft.com/azure/azure-functions/)
- [Stripe Webhooks](https://stripe.com/docs/webhooks)

