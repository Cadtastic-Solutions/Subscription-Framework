using System.Threading.Tasks;
using LicenseService.Models;

namespace LicenseService.Interfaces
{
    /// <summary>
    /// Service interface for managing license validation and operations
    /// </summary>
    public interface ILicenseService
    {
        /// <summary>
        /// Validates a license key against the provided request details
        /// </summary>
        /// <param name="subscriptionKey">The APIM subscription key to validate</param>
        /// <param name="request">The validation request containing App GUID and optional feature ID</param>
        /// <returns>A response indicating if the license is valid and associated details</returns>
        Task<LicenseValidationResponse> ValidateLicenseAsync(string subscriptionKey, LicenseValidationRequest request);
        
        /// <summary>
        /// Generates a cryptographically signed offline token for use when the application is offline
        /// </summary>
        /// <param name="subscriptionKey">The APIM subscription key to generate a token for</param>
        /// <param name="request">The request containing App GUID and token parameters</param>
        /// <returns>A response containing the signed token or error information</returns>
        Task<OfflineTokenResponse> GenerateOfflineTokenAsync(string subscriptionKey, OfflineTokenRequest request);
        
        /// <summary>
        /// Processes Stripe webhook events to update license status based on subscription changes
        /// </summary>
        /// <param name="json">The raw JSON webhook payload from Stripe</param>
        /// <param name="stripeSignature">The Stripe signature header for validating webhook authenticity</param>
        /// <returns>True if the webhook was processed successfully, false otherwise</returns>
        Task<bool> HandleStripeEventAsync(string json, string stripeSignature);
    }
} 