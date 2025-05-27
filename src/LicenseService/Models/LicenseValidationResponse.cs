using System;
using System.Collections.Generic;

namespace LicenseService.Models
{
    /// <summary>
    /// Represents a response to a license validation request
    /// </summary>
    public class LicenseValidationResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the license is valid
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Gets or sets the status code for the validation result (active, expired, etc.)
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets a human-readable message describing the validation result
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the license plan name
        /// </summary>
        public string? Plan { get; set; }
        
        /// <summary>
        /// Gets or sets the list of features enabled for this license
        /// </summary>
        public List<string> Features { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the date when the license expires, if applicable
        /// </summary>
        public DateTime? ExpirationDate { get; set; }
        
        /// <summary>
        /// Gets or sets the number of days remaining in the trial, if applicable
        /// </summary>
        public int? TrialDaysRemaining { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether a payment is due for this license
        /// </summary>
        public bool PaymentDue { get; set; }
    }
} 