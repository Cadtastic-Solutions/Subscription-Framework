# Welcome to Subscription Framework

Welcome to the **Subscription Framework** project! This repository is part of Cadtastic Solutions and is designed to provide a reference implementation for managing software licenses and subscriptions using Azure and Stripe. Here you will find all the documentation, code, and resources needed to get started.

---

## Table of Contents

- [About the Project](#about-the-project)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

---

## About the Project

The Subscription Framework demonstrates how a software vendor can securely issue and validate license keys while handling subscription billing events. It combines Azure API Management, a custom License Service, and Stripe to manage trials, pauses, cancellations, and offline grace periods. This project serves as a starting point for building a scalable licensing platform.

---

## Getting Started

Follow these steps to set up and run the project locally:

### Prerequisites
- **Git** to clone the repository.
- **.NET 6 SDK or later** for building and running the sample code.
- Optionally install the **Azure CLI** if you plan to deploy the services.

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/Cadtastic-Solutions/Subscription-Framework.git
   ```
2. Navigate to the project directory:
   ```bash
   cd Subscription-Framework
   ```
3. Install dependencies:
   ```bash
   dotnet restore
   ```

---

## Usage

After restoring dependencies you can run the sample License Service locally to experiment with the API and Stripe webhook handling.

### Example Usage
```bash
dotnet run
```

---

## Contributing

We welcome contributions! To contribute:
1. Fork the repository.
2. Create a feature branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. Commit your changes:
   ```bash
   git commit -m "Add your commit message"
   ```
4. Push your changes:
   ```bash
   git push origin feature/your-feature-name
   ```
5. Open a pull request.

For more details, see our [Contributing Guidelines](CONTRIBUTING.md).

---

## License

This project is licensed under the **Apache License 2.0**. See the [LICENSE](LICENSE) file for details.

---

## Contact

For questions or support, please reach out to:
- **Addam Boord**
- Email: addam.boord@cadtasticsolutions.com
- Open an issue on our [GitHub repository](https://github.com/Cadtastic-Solutions/Subscription-Framework/issues)

---
Subscription Framework is maintained by Cadtastic Solutions.
