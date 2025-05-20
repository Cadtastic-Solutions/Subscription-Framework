# ClientSdk

Cross-platform library that simplifies communication with the License Service. It implements online validation calls via Azure API Management and verifies offline tokens as described in the [Architecture Overview](../../README.md).

## Features
- Validate license keys using `HttpClient`
- Request and verify signed offline tokens (public key bundled)
- Helper methods to store license state locally

Use this library from desktop or plugin applications to ensure consistent integration with the licensing backend.
