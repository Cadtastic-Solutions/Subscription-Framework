using Microsoft.AspNetCore.Mvc;

namespace LicenseService.Controllers
{
    /// <summary>
    /// Controller for health check endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Gets the health status of the API
        /// </summary>
        /// <returns>A 200 OK response with "Healthy" status if the API is running</returns>
        [HttpGet]
        public IActionResult Get() => Ok(new { status = "Healthy" });
    }
}
