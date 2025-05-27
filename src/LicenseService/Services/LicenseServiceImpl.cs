using System;
using System.Threading.Tasks;
using LicenseService.Interfaces;
using LicenseService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace LicenseService.Services
{
    /// <summary>
    /// Main implementation of the license service that handles license validation, 
    /// offline token generation, and Stripe event processing
    /// </summary>
    public class LicenseServiceImpl : ILicenseService
    {
        private readonly ILicenseRepository _repository;
        private readonly ICryptoService _cryptoService;
        private readonly ILogger<LicenseServiceImpl> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseServiceImpl"/> class
        /// </summary>
        /// <param name="repository">The license repository for data persistence</param>
        /// <param name="cryptoService">The crypto service for token operations</param>
        /// <param name="logger">The logger for application insights</param>
        /// <param name="configuration">Application configuration</param>
        public LicenseServiceImpl(
            ILicenseRepository repository,
            ICryptoService cryptoService,
            ILogger<LicenseServiceImpl> logger,
            IConfiguration configuration)
        {
            _repository = repository;
            _cryptoService = cryptoService;
            _logger = logger;
            _configuration = configuration;
            
            // Initialize Stripe
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        /// <inheritdoc />
        public async Task<LicenseValidationResponse> ValidateLicenseAsync(string subscriptionKey, LicenseValidationRequest request)
        {
            try
            {
                var license = await _repository.GetLicenseBySubscriptionKeyAsync(subscriptionKey);
                
                if (license == null)
                {
                    _logger.LogWarning("License not found for key: {SubscriptionKey}", subscriptionKey);
                    return new LicenseValidationResponse
                    {
                        IsValid = false,
                        Status = "not_found",
                        Message = "License key not found"
                    };
                }

                // Check if App GUID matches
                if (license.AppGuid != request.AppGuid)
                {
                    _logger.LogWarning("App GUID mismatch for key: {SubscriptionKey}. Expected: {ExpectedGuid}, Actual: {ActualGuid}", 
                        subscriptionKey, license.AppGuid, request.AppGuid);
                    return new LicenseValidationResponse
                    {
                        IsValid = false,
                        Status = "app_guid_mismatch",
                        Message = "This license key is not valid for this application"
                    };
                }

                // Check license status
                if (license.Status != LicenseStatus.Active && license.Status != LicenseStatus.PastDue)
                {
                    return new LicenseValidationResponse
                    {
                        IsValid = false,
                        Status = license.Status.ToString().ToLower(),
                        Message = $"License is {license.Status}"
                    };
                }

                // Check expiration
                if (license.ExpirationDate.HasValue && license.ExpirationDate.Value < DateTime.UtcNow)
                {
                    license.Status = LicenseStatus.Expired;
                    await _repository.UpdateLicenseAsync(license);
                    
                    return new LicenseValidationResponse
                    {
                        IsValid = false,
                        Status = "expired",
                        Message = "License has expired"
                    };
                }

                // Check trial status
                if (license.IsTrial && license.TrialDaysRemaining.HasValue)
                {
                    if (license.TrialDaysRemaining <= 0)
                    {
                        license.Status = LicenseStatus.TrialExpired;
                        await _repository.UpdateLicenseAsync(license);
                        
                        return new LicenseValidationResponse
                        {
                            IsValid = false,
                            Status = "trial_expired",
                            Message = "Trial period has expired"
                        };
                    }
                }

                // Check feature entitlement if requested
                if (!string.IsNullOrEmpty(request.FeatureId) && !license.Features.Contains(request.FeatureId))
                {
                    return new LicenseValidationResponse
                    {
                        IsValid = false,
                        Status = "feature_not_entitled",
                        Message = $"License does not include access to feature: {request.FeatureId}"
                    };
                }

                // Update last validation date
                license.LastValidationDate = DateTime.UtcNow;
                await _repository.UpdateLicenseAsync(license);

                // License is valid
                return new LicenseValidationResponse
                {
                    IsValid = true,
                    Status = license.Status == LicenseStatus.PastDue ? "past_due" : "active",
                    Message = "License is valid",
                    Plan = license.IsTrial ? "Trial" : "Pro", // This would come from license data in a real implementation
                    Features = license.Features,
                    ExpirationDate = license.ExpirationDate,
                    TrialDaysRemaining = license.TrialDaysRemaining,
                    PaymentDue = license.Status == LicenseStatus.PastDue
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating license: {SubscriptionKey}", subscriptionKey);
                return new LicenseValidationResponse
                {
                    IsValid = false,
                    Status = "error",
                    Message = "An error occurred while validating the license"
                };
            }
        }

        /// <inheritdoc />
        public async Task<OfflineTokenResponse> GenerateOfflineTokenAsync(string subscriptionKey, OfflineTokenRequest request)
        {
            try
            {
                var license = await _repository.GetLicenseBySubscriptionKeyAsync(subscriptionKey);
                
                if (license == null)
                {
                    _logger.LogWarning("License not found for key: {SubscriptionKey}", subscriptionKey);
                    return new OfflineTokenResponse
                    {
                        Success = false,
                        Message = "License key not found"
                    };
                }

                // Check if App GUID matches
                if (license.AppGuid != request.AppGuid)
                {
                    _logger.LogWarning("App GUID mismatch for offline token request. Expected: {ExpectedGuid}, Actual: {ActualGuid}", 
                        license.AppGuid, request.AppGuid);
                    return new OfflineTokenResponse
                    {
                        Success = false,
                        Message = "This license key is not valid for this application"
                    };
                }

                // Check license status
                if (license.Status != LicenseStatus.Active && license.Status != LicenseStatus.PastDue)
                {
                    return new OfflineTokenResponse
                    {
                        Success = false,
                        Message = $"Cannot issue offline token. License is {license.Status}"
                    };
                }

                // Calculate token expiration
                // Get configured offline grace period (default 14 days)
                int maxOfflineDays = _configuration.GetValue<int>("License:MaxOfflineDays", 14);
                
                // Use the smaller of requested duration or max allowed
                int durationDays = request.RequestedDurationDays.HasValue ? 
                    Math.Min(request.RequestedDurationDays.Value, maxOfflineDays) : 
                    maxOfflineDays;
                
                // Calculate expiration date
                DateTime expirationDate = DateTime.UtcNow.AddDays(durationDays);
                
                // If license expires sooner, use that date instead
                if (license.ExpirationDate.HasValue && license.ExpirationDate.Value < expirationDate)
                {
                    expirationDate = license.ExpirationDate.Value;
                }

                // Create token payload
                var tokenPayload = new OfflineToken
                {
                    LicenseId = license.Id,
                    AppGuid = license.AppGuid,
                    DeviceId = request.DeviceId,
                    IssuedAt = DateTime.UtcNow,
                    ExpiresAt = expirationDate,
                    Features = license.Features,
                    Plan = license.IsTrial ? "Trial" : "Pro" // This would come from license data in a real implementation
                };

                // Sign the token
                string token = await _cryptoService.SignTokenAsync(tokenPayload);

                // Update license with last token issuance info
                license.LastOfflineTokenIssuedDate = DateTime.UtcNow;
                await _repository.UpdateLicenseAsync(license);

                return new OfflineTokenResponse
                {
                    Success = true,
                    Token = token,
                    ExpirationDate = expirationDate,
                    Message = $"Offline token valid until {expirationDate:G}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating offline token: {SubscriptionKey}", subscriptionKey);
                return new OfflineTokenResponse
                {
                    Success = false,
                    Message = "An error occurred while generating the offline token"
                };
            }
        }

        /// <inheritdoc />
        public async Task<bool> HandleStripeEventAsync(string json, string stripeSignature)
        {
            try
            {
                // Verify webhook signature
                var webhookSecret = _configuration["Stripe:WebhookSecret"];
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

                switch (stripeEvent.Type)
                {
                    case Events.CheckoutSessionCompleted:
                        var session = stripeEvent.Data.Object as Session;
                        await HandleCheckoutSessionCompletedAsync(session);
                        break;

                    case Events.InvoicePaymentSucceeded:
                        var invoice = stripeEvent.Data.Object as Invoice;
                        await HandleInvoicePaymentSucceededAsync(invoice);
                        break;

                    case Events.InvoicePaymentFailed:
                        var failedInvoice = stripeEvent.Data.Object as Invoice;
                        await HandleInvoicePaymentFailedAsync(failedInvoice);
                        break;

                    case Events.CustomerSubscriptionUpdated:
                        var subscription = stripeEvent.Data.Object as Subscription;
                        await HandleSubscriptionUpdatedAsync(subscription);
                        break;

                    case Events.CustomerSubscriptionDeleted:
                        var deletedSubscription = stripeEvent.Data.Object as Subscription;
                        await HandleSubscriptionDeletedAsync(deletedSubscription);
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Stripe webhook event");
                return false;
            }
        }

        /// <summary>
        /// Handles the checkout.session.completed event from Stripe
        /// </summary>
        /// <param name="session">The Stripe checkout session</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task HandleCheckoutSessionCompletedAsync(Session session)
        {
            if (session == null || session.SubscriptionId == null)
                return;

            // Here you would typically:
            // 1. Create a new license record
            // 2. Call APIM to create a subscription/key
            // 3. Send the license key to the customer
            
            // This is a simplified example:
            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.GetAsync(session.SubscriptionId);
            
            // Create a new license
            var license = new License
            {
                StripeSubscriptionId = subscription.Id,
                // In a real implementation, you would get the APIM subscription key
                // and product info from APIM or your database
                SubscriptionKey = Guid.NewGuid().ToString(), // Placeholder
                AppGuid = "your-app-guid", // This would come from the checkout metadata
                Status = LicenseStatus.Active,
                Features = new System.Collections.Generic.List<string> { "feature1", "feature2" }
            };
            
            await _repository.CreateLicenseAsync(license);
            
            _logger.LogInformation("Created new license for subscription: {SubscriptionId}", subscription.Id);
        }

        /// <summary>
        /// Handles the invoice.payment_succeeded event from Stripe
        /// </summary>
        /// <param name="invoice">The Stripe invoice</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task HandleInvoicePaymentSucceededAsync(Invoice invoice)
        {
            if (invoice?.SubscriptionId == null)
                return;

            var license = await _repository.GetLicenseByStripeSubscriptionIdAsync(invoice.SubscriptionId);
            if (license != null)
            {
                // Update license status if it was past due
                if (license.Status == LicenseStatus.PastDue)
                {
                    license.Status = LicenseStatus.Active;
                    await _repository.UpdateLicenseAsync(license);
                    _logger.LogInformation("Reactivated license for subscription: {SubscriptionId}", invoice.SubscriptionId);
                }
            }
        }

        /// <summary>
        /// Handles the invoice.payment_failed event from Stripe
        /// </summary>
        /// <param name="invoice">The Stripe invoice</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task HandleInvoicePaymentFailedAsync(Invoice invoice)
        {
            if (invoice?.SubscriptionId == null)
                return;

            var license = await _repository.GetLicenseByStripeSubscriptionIdAsync(invoice.SubscriptionId);
            if (license != null)
            {
                // Mark license as past due
                license.Status = LicenseStatus.PastDue;
                await _repository.UpdateLicenseAsync(license);
                _logger.LogInformation("Marked license as past due for subscription: {SubscriptionId}", invoice.SubscriptionId);
            }
        }

        /// <summary>
        /// Handles the customer.subscription.updated event from Stripe
        /// </summary>
        /// <param name="subscription">The Stripe subscription</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task HandleSubscriptionUpdatedAsync(Subscription subscription)
        {
            if (subscription == null)
                return;

            var license = await _repository.GetLicenseByStripeSubscriptionIdAsync(subscription.Id);
            if (license != null)
            {
                // Update license status based on subscription status
                switch (subscription.Status)
                {
                    case "active":
                        license.Status = LicenseStatus.Active;
                        break;
                    case "past_due":
                        license.Status = LicenseStatus.PastDue;
                        break;
                    case "canceled":
                        license.Status = LicenseStatus.Canceled;
                        break;
                    case "unpaid":
                        license.Status = LicenseStatus.Canceled;
                        break;
                    case "paused":
                        license.Status = LicenseStatus.Paused;
                        break;
                }

                await _repository.UpdateLicenseAsync(license);
                _logger.LogInformation("Updated license status to {Status} for subscription: {SubscriptionId}", 
                    license.Status, subscription.Id);
            }
        }

        /// <summary>
        /// Handles the customer.subscription.deleted event from Stripe
        /// </summary>
        /// <param name="subscription">The Stripe subscription</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task HandleSubscriptionDeletedAsync(Subscription subscription)
        {
            if (subscription == null)
                return;

            var license = await _repository.GetLicenseByStripeSubscriptionIdAsync(subscription.Id);
            if (license != null)
            {
                // Mark license as canceled
                license.Status = LicenseStatus.Canceled;
                await _repository.UpdateLicenseAsync(license);
                _logger.LogInformation("Marked license as canceled for deleted subscription: {SubscriptionId}", subscription.Id);
            }
        }
    }
} 