using Microsoft.AspNetCore.Mvc;

namespace Circle.WebApi.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            service = "Circle.WebApi",
            status = "Healthy",
            checkedAtUtc = DateTime.UtcNow
        });
    }
}
