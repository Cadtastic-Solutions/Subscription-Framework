# User Manual: Managing Subscriptions

This manual describes how administrators can manage customer subscriptions when using the Subscription Framework. It focuses on the Stripe integration developed in Phase 2 of the project plan.

## Overview

Customers purchase plans through Stripe. Webhook events trigger automated license creation in Azure API Management (APIM) and update records in the License Service database.

## Admin Tasks

### Viewing Subscriptions
1. Sign in to the Stripe Dashboard and open the **Customers** section to see active subscriptions.
2. The License Service dashboard (to be developed) will also display subscription status, payment history, and associated APIM keys.

### Pausing or Canceling
- To pause billing, use Stripe's **Pause collection** feature. The webhook handler marks the license as suspended, preventing API access.
- To cancel, cancel the subscription in Stripe. The webhook handler deactivates the license and, if desired, deletes the APIM subscription.

### Payment Failures
- Stripe automatically retries failed payments according to your settings.
- After the retry schedule, the webhook handler marks the license inactive. You can manually reactivate once payment issues are resolved.

### Issuing Trials
- Create a price with a trial period or a $0 plan in Stripe.
- When the trial starts, a license is issued just like a paid plan but flagged as trial in the database.

## Customer Experience

1. Customer completes checkout and receives a confirmation email containing the API key.
2. The client application uses this key along with the `X-App-Id` header for license validation via APIM.
3. If payment lapses or the subscription is canceled, subsequent validations will fail after any configured grace period.

## Support

For issues with subscription status or billing, administrators should consult both Stripe logs and the License Service logs. Common problems include webhook secret mismatch and API failures when creating APIM subscriptions.
