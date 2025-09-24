using Microsoft.AspNetCore.Mvc;

namespace EthanRushbrook.TempSense.Controllers;

[ApiController]
[Route("/")]
public class BaseController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("Server is OK");
}
