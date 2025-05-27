using System.Threading.Tasks;
using LicenseService.Interfaces;
using LicenseService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LicenseService.Controllers
{
    /// <summary>
    /// Controller for license validation and offline token operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LicenseController : ControllerBase
    {
        private readonly ILicenseService _licenseService;
        private readonly ILogger<LicenseController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseController"/> class
        /// </summary>
        /// <param name="licenseService">The license service</param>
        /// <param name="logger">The logger</param>
        public LicenseController(ILicenseService licenseService, ILogger<LicenseController> logger)
        {
            _licenseService = licenseService;
            _logger = logger;
        }

        /// <summary>
        /// Validates a license key for an application or feature
        /// </summary>
        /// <param name="request">The validation request containing the App GUID and optional feature ID</param>
        /// <returns>A validation response indicating if the license is valid and its details</returns>
        /// <remarks>
        /// The subscription key must be provided in the Ocp-Apim-Subscription-Key header.
        /// In production, this header would typically be handled by Azure API Management.
        /// </remarks>
        [HttpPost("validate")]
        public async Task<ActionResult<LicenseValidationResponse>> ValidateLicense(
            [FromBody] LicenseValidationRequest request)
        {
            // Get subscription key from header (in production, this would be handled by APIM)
            if (!Request.Headers.TryGetValue("Ocp-Apim-Subscription-Key", out var subscriptionKey) || 
                string.IsNullOrEmpty(subscriptionKey))
            {
                return BadRequest(new { message = "Missing subscription key" });
            }

            var response = await _licenseService.ValidateLicenseAsync(subscriptionKey, request);
            
            if (!response.IsValid)
            {
                // Log validation failure (without sensitive details)
                _logger.LogWarning("License validation failed: {Status}", response.Status);
                
                // Return 403 Forbidden for invalid licenses
                return StatusCode(403, response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Generates an offline token for use when the application will be offline
        /// </summary>
        /// <param name="request">The request containing the App GUID and token parameters</param>
        /// <returns>A response containing the signed offline token or error information</returns>
        /// <remarks>
        /// The subscription key must be provided in the Ocp-Apim-Subscription-Key header.
        /// </remarks>
        [HttpPost("offlineToken")]
        public async Task<ActionResult<OfflineTokenResponse>> GetOfflineToken(
            [FromBody] OfflineTokenRequest request)
        {
            // Get subscription key from header
            if (!Request.Headers.TryGetValue("Ocp-Apim-Subscription-Key", out var subscriptionKey) || 
                string.IsNullOrEmpty(subscriptionKey))
            {
                return BadRequest(new { message = "Missing subscription key" });
            }

            var response = await _licenseService.GenerateOfflineTokenAsync(subscriptionKey, request);
            
            if (!response.Success)
            {
                // Log token generation failure
                _logger.LogWarning("Offline token generation failed: {Message}", response.Message);
                
                // Return 403 Forbidden for invalid licenses
                return StatusCode(403, response);
            }

            return Ok(response);
        }
    }
} 