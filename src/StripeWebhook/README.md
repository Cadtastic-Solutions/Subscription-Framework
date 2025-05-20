# StripeWebhook

Azure Function written in Node.js that receives events from Stripe and forwards them to the License Service. This function ensures subscription changes are reflected in the licensing database.

It verifies the webhook signature using the Stripe secret and should be configured with environment variables as described in the [Architecture Overview](../../README.md).
