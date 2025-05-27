using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LicenseService.Interfaces;
using LicenseService.Models;

namespace LicenseService.Services
{
    /// <summary>
    /// In-memory implementation of the license repository for development and testing purposes
    /// </summary>
    /// <remarks>
    /// This implementation stores all licenses in memory and does not persist data across application restarts.
    /// In a production environment, this should be replaced with a database-backed implementation.
    /// </remarks>
    public class InMemoryLicenseRepository : ILicenseRepository
    {
        private readonly List<License> _licenses = new();

        /// <inheritdoc />
        public Task<License> CreateLicenseAsync(License license)
        {
            license.Id = Guid.NewGuid().ToString();
            _licenses.Add(license);
            return Task.FromResult(license);
        }

        /// <inheritdoc />
        public Task<bool> DeleteLicenseAsync(string licenseId)
        {
            var license = _licenses.FirstOrDefault(l => l.Id == licenseId);
            if (license != null)
            {
                _licenses.Remove(license);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<License?> GetLicenseBySubscriptionKeyAsync(string subscriptionKey)
        {
            return Task.FromResult(_licenses.FirstOrDefault(l => l.SubscriptionKey == subscriptionKey));
        }

        /// <inheritdoc />
        public Task<License?> GetLicenseByStripeSubscriptionIdAsync(string stripeSubscriptionId)
        {
            return Task.FromResult(_licenses.FirstOrDefault(l => l.StripeSubscriptionId == stripeSubscriptionId));
        }

        /// <inheritdoc />
        public Task<bool> UpdateLicenseAsync(License license)
        {
            var existingLicense = _licenses.FirstOrDefault(l => l.Id == license.Id);
            if (existingLicense != null)
            {
                var index = _licenses.IndexOf(existingLicense);
                _licenses[index] = license;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
} 