namespace LicenseService.Models
{
    /// <summary>
    /// Represents a request to generate an offline token
    /// </summary>
    public class OfflineTokenRequest
    {
        /// <summary>
        /// Gets or sets the GUID of the application or feature for the offline token
        /// </summary>
        public string AppGuid { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the requested duration in days for the offline token
        /// </summary>
        /// <remarks>
        /// The server may override this with a maximum allowed duration.
        /// </remarks>
        public int? RequestedDurationDays { get; set; }
        
        /// <summary>
        /// Gets or sets the device identifier for creating device-specific tokens
        /// </summary>
        public string? DeviceId { get; set; }
    }
} 