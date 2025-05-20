# Python Sample Implementation

This example uses the Python `http.server` module to host a very small license service.

## Running
```bash
python main.py
```
The server listens on port 7071 and exposes `/validate` and `/offlineToken` endpoints.

In production you would implement these features using an Azure Function or Flask app and secure the signing key using Azure Key Vault.
