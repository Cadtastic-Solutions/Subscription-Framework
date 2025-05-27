using System;

namespace LicenseService.Models
{
    /// <summary>
    /// Represents a response to an offline token request
    /// </summary>
    public class OfflineTokenResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the token was successfully generated
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets or sets the signed JWT token for offline validation
        /// </summary>
        public string? Token { get; set; }
        
        /// <summary>
        /// Gets or sets the date when the token expires
        /// </summary>
        public DateTime? ExpirationDate { get; set; }
        
        /// <summary>
        /// Gets or sets a message describing the result of the token request
        /// </summary>
        public string? Message { get; set; }
    }
} 