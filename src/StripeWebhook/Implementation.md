# StripeWebhook Implementation

This Function App is created using the Azure Functions Node.js runtime.

## 1. Scaffold the Function
```bash
func init StripeWebhook --worker-runtime node
cd StripeWebhook
func new --template "HTTP trigger" --name webhook
```
Replace the generated code with logic that validates the Stripe signature and posts the payload to the License Service.

## 2. Install Dependencies
```bash
npm install stripe axios
```

## 3. Local Settings
Create `local.settings.json` with your Stripe secret and License Service endpoint. Never commit this file.

## 4. Run Locally
```bash
func start
```
The function listens on port 7071 and can be configured as a webhook endpoint in your Stripe dashboard.
