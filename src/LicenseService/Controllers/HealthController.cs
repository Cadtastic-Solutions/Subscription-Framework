using Microsoft.AspNetCore.Mvc;

namespace LicenseService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { status = "Healthy" });
    }
}
