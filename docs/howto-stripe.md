# How To: Integrate Stripe and Automate Subscriptions

This guide explains how to configure Stripe and Azure resources to automate license provisioning for the Subscription Framework. It complements the planning document's Phase 2 tasks.

## Prerequisites

- A [Stripe](https://stripe.com/docs) account with API keys
- An Azure subscription
- Azure CLI with Bicep installed

## Steps

1. **Create Stripe Products**
   - In the Stripe Dashboard, create Products and recurring Prices that match your license plans.
   - Note the price IDs; they will be used in checkout links or API calls.

2. **Deploy Webhook Handler**
   - Use the Bicep template in `templates/stripe-subscription.bicep` to deploy a Function App configured for webhook processing.
   - Example deployment command:
     ```bash
     az deployment group create \
       --resource-group <rg> \
       --template-file templates/stripe-subscription.bicep \
       --parameters storageAccountName=<name> \
                    functionAppName=<app-name> \
                    appServicePlanName=<plan-name> \
                    apimServiceName=<apim-name>
     ```
   - Set the `STRIPE_SECRET` and `STRIPE_WEBHOOK_SECRET` application settings for the Function App.

3. **Configure Stripe Webhook**
   - In Stripe, add an endpoint pointing to `https://<functionAppUrl>/api/stripe/webhook`.
   - Subscribe to events such as `invoice.paid`, `customer.subscription.created`, and `invoice.payment_failed`.

4. **Automate APIM Subscription Creation**
   - The webhook handler should call the Azure API Management REST API to create or update subscriptions when payments succeed.
   - Refer to [APIM REST documentation](https://learn.microsoft.com/azure/api-management/api-management-howto-develop-with-rest) for required headers and authentication.

5. **Testing**
   - Use Stripe's CLI or dashboard to send test webhook events to the Function App.
   - Verify that licenses are created or updated in your database and APIM.

## Next Steps

After the webhook flow is verified, integrate this with your License Service for full lifecycle management.
