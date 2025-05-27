using System;
using System.IO;
using System.Threading.Tasks;
using LicenseService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LicenseService.Controllers
{
    /// <summary>
    /// Controller for handling Stripe webhook events
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ILicenseService _licenseService;
        private readonly ILogger<StripeWebhookController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StripeWebhookController"/> class
        /// </summary>
        /// <param name="licenseService">The license service</param>
        /// <param name="logger">The logger</param>
        public StripeWebhookController(ILicenseService licenseService, ILogger<StripeWebhookController> logger)
        {
            _licenseService = licenseService;
            _logger = logger;
        }

        /// <summary>
        /// Processes webhook events from Stripe
        /// </summary>
        /// <returns>An HTTP 200 OK response if the webhook was processed successfully</returns>
        /// <remarks>
        /// This endpoint handles various Stripe events including:
        /// - checkout.session.completed
        /// - invoice.payment_succeeded
        /// - invoice.payment_failed
        /// - customer.subscription.updated
        /// - customer.subscription.deleted
        /// 
        /// The Stripe-Signature header is required for webhook authentication.
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            try
            {
                // Read the request body
                using var reader = new StreamReader(HttpContext.Request.Body);
                var json = await reader.ReadToEndAsync();

                // Get the Stripe signature header
                if (!Request.Headers.TryGetValue("Stripe-Signature", out var stripeSignature))
                {
                    _logger.LogWarning("Missing Stripe signature header");
                    return BadRequest(new { message = "Missing Stripe signature header" });
                }

                // Process the webhook
                var success = await _licenseService.HandleStripeEventAsync(json, stripeSignature);
                
                if (!success)
                {
                    _logger.LogWarning("Failed to process Stripe webhook");
                    return StatusCode(500, new { message = "Failed to process webhook" });
                }

                return Ok(new { received = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook");
                return StatusCode(500, new { message = "Error processing webhook" });
            }
        }
    }
} 