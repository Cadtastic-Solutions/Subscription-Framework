using System;
using System.Collections.Generic;

namespace LicenseService.Models
{
    /// <summary>
    /// Represents a license with its metadata and status
    /// </summary>
    public class License
    {
        /// <summary>
        /// Gets or sets the unique identifier for the license
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the APIM subscription key associated with this license
        /// </summary>
        public string SubscriptionKey { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the application GUID that this license is valid for
        /// </summary>
        public string AppGuid { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the user ID of the license owner
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the Stripe subscription ID associated with this license
        /// </summary>
        public string StripeSubscriptionId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the current status of the license
        /// </summary>
        public LicenseStatus Status { get; set; } = LicenseStatus.Active;
        
        /// <summary>
        /// Gets or sets the date when the license was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Gets or sets the date when the license expires, if applicable
        /// </summary>
        public DateTime? ExpirationDate { get; set; }
        
        /// <summary>
        /// Gets or sets the date when the license was last validated online
        /// </summary>
        public DateTime? LastValidationDate { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this license is a trial
        /// </summary>
        public bool IsTrial { get; set; }
        
        /// <summary>
        /// Gets or sets the number of days remaining in the trial, if applicable
        /// </summary>
        public int? TrialDaysRemaining { get; set; }
        
        /// <summary>
        /// Gets or sets the features enabled for this license
        /// </summary>
        public List<string> Features { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the date when an offline token was last issued for this license
        /// </summary>
        public DateTime? LastOfflineTokenIssuedDate { get; set; }
    }

    /// <summary>
    /// Represents the possible states of a license
    /// </summary>
    public enum LicenseStatus
    {
        /// <summary>
        /// The license is active and valid
        /// </summary>
        Active,
        
        /// <summary>
        /// The license is temporarily paused
        /// </summary>
        Paused,
        
        /// <summary>
        /// The license has a payment due but is still in the grace period
        /// </summary>
        PastDue,
        
        /// <summary>
        /// The license has been canceled
        /// </summary>
        Canceled,
        
        /// <summary>
        /// The license has expired
        /// </summary>
        Expired,
        
        /// <summary>
        /// The trial period for this license has expired
        /// </summary>
        TrialExpired
    }
} 