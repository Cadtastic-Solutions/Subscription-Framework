namespace LicenseService.Models
{
    /// <summary>
    /// Represents a request to validate a license key
    /// </summary>
    public class LicenseValidationRequest
    {
        /// <summary>
        /// Gets or sets the GUID of the application or feature being validated
        /// </summary>
        public string AppGuid { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the specific feature ID to check entitlement for, if applicable
        /// </summary>
        public string? FeatureId { get; set; }
    }
} 