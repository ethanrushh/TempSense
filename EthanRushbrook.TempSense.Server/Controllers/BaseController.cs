using Microsoft.AspNetCore.Mvc;

namespace EthanRushbrook.TempSense.Server.Controllers;

[ApiController]
[Route("/")]
public class BaseController : ControllerBase
{
    private readonly ILogger<BaseController> _logger;

    public BaseController(ILogger<BaseController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get() => Ok("Server is OK");
}
