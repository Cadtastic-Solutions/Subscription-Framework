using System.Threading.Tasks;
using LicenseService.Models;

namespace LicenseService.Interfaces
{
    /// <summary>
    /// Service interface for cryptographic operations used in license token generation and verification
    /// </summary>
    public interface ICryptoService
    {
        /// <summary>
        /// Signs an offline token with a cryptographic key to prevent tampering
        /// </summary>
        /// <param name="token">The offline token payload to sign</param>
        /// <returns>A signed JWT token string</returns>
        Task<string> SignTokenAsync(OfflineToken token);
        
        /// <summary>
        /// Verifies that a token's signature is valid and has not been tampered with
        /// </summary>
        /// <param name="token">The token string to verify</param>
        /// <returns>True if the token signature is valid, false otherwise</returns>
        Task<bool> VerifyTokenSignatureAsync(string token);
    }
} 