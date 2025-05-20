# Welcome to Subscription Framework

Welcome to the **Subscription Framework** project! This repository is part of Cadtastic Solutions and provides a reference implementation for a subscription‑based licensing system. Here you will find the source code, architecture documentation, and planning resources to help you deploy and extend the framework.

---

## Table of Contents

- [About the Project](#about-the-project)
- [Project Planning](#project-planning)
- [License Validation & Security](license-validation-security.md)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

---

## About the Project

Subscription Framework provides a fully featured licensing service built on Azure. It integrates **Stripe** for billing, **Azure API Management** for key management, and a custom **License Service** to enforce online and offline validations. The goal is to give software vendors a secure, repeatable foundation for managing trials, paid plans, and feature entitlements across desktop and cloud applications.

---

## Project Planning

Detailed planning information, including the overall scope, development phases, and a proposed timeline, is available in [Project Planning](planning.md).
Guidance for implementing secure license validation is provided in [License Validation & Security](license-validation-security.md).

---

## Getting Started

Follow these steps to set up and run the project locally:

### Prerequisites
- [Git](https://git-scm.com/) for version control
- [.NET 6 SDK](https://dotnet.microsoft.com/download) installed
- An Azure account with permissions to create resources
- A Stripe account for testing billing flows

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

After restoring dependencies you can run the sample License Service locally:

```bash
dotnet run
```

The service exposes basic endpoints for license validation which can be called through Azure API Management once deployed.

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
- **Addam Boord** – [addam.boord@cadtasticsolutions.com](mailto:addam.boord@cadtasticsolutions.com)
- You can also open issues on the project repository if you encounter problems or have feature requests.

---
Subscription Framework is maintained by Cadtastic Solutions.
