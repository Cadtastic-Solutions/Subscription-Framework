using System;
using System.Collections.Generic;

namespace LicenseService.Models
{
    /// <summary>
    /// Represents the payload for an offline token that will be signed with a JWT
    /// </summary>
    public class OfflineToken
    {
        /// <summary>
        /// Gets or sets the license ID associated with this token
        /// </summary>
        public string LicenseId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the application GUID that this token is valid for
        /// </summary>
        public string AppGuid { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the device ID for device-specific tokens
        /// </summary>
        public string? DeviceId { get; set; }
        
        /// <summary>
        /// Gets or sets the date when the token was issued
        /// </summary>
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Gets or sets the date when the token expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Gets or sets the list of features enabled by this token
        /// </summary>
        public List<string> Features { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the license plan name
        /// </summary>
        public string? Plan { get; set; }
    }
} 