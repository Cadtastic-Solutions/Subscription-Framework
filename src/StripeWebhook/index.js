const stripe = require('stripe')(process.env.STRIPE_SECRET);

module.exports = async function (context, req) {
  const sig = req.headers['stripe-signature'];
  const body = req.rawBody;

  let event;
  try {
    event = stripe.webhooks.constructEvent(body, sig, process.env.STRIPE_WEBHOOK_SECRET);
  } catch (err) {
    context.log('Webhook signature verification failed.', err.message);
    context.res = { status: 400 };
    return;
  }

  context.log(`Received event type ${event.type}`);

  switch (event.type) {
    case 'invoice.paid':
      // Handle successful payment
      break;
    case 'customer.subscription.created':
      // Provision license on new subscription
      break;
    case 'invoice.payment_failed':
      // Flag payment failure
      break;
    default:
      context.log(`Unhandled event type ${event.type}`);
  }

  context.res = { status: 200 };
};
