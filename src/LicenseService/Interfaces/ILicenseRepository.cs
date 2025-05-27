using System.Threading.Tasks;
using LicenseService.Models;

namespace LicenseService.Interfaces
{
    /// <summary>
    /// Repository interface for license data persistence operations
    /// </summary>
    public interface ILicenseRepository
    {
        /// <summary>
        /// Retrieves a license by its APIM subscription key
        /// </summary>
        /// <param name="subscriptionKey">The subscription key to search for</param>
        /// <returns>The license if found, or null if not found</returns>
        Task<License?> GetLicenseBySubscriptionKeyAsync(string subscriptionKey);
        
        /// <summary>
        /// Updates an existing license record
        /// </summary>
        /// <param name="license">The license with updated values</param>
        /// <returns>True if the update was successful, false otherwise</returns>
        Task<bool> UpdateLicenseAsync(License license);
        
        /// <summary>
        /// Creates a new license record
        /// </summary>
        /// <param name="license">The license to create</param>
        /// <returns>The created license with populated ID</returns>
        Task<License> CreateLicenseAsync(License license);
        
        /// <summary>
        /// Deletes a license by its ID
        /// </summary>
        /// <param name="licenseId">The ID of the license to delete</param>
        /// <returns>True if the license was deleted, false if not found</returns>
        Task<bool> DeleteLicenseAsync(string licenseId);
        
        /// <summary>
        /// Retrieves a license by its associated Stripe subscription ID
        /// </summary>
        /// <param name="stripeSubscriptionId">The Stripe subscription ID to search for</param>
        /// <returns>The license if found, or null if not found</returns>
        Task<License?> GetLicenseByStripeSubscriptionIdAsync(string stripeSubscriptionId);
    }
} 