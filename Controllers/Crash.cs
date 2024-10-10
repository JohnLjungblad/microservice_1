using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api")]
public class Crash : ControllerBase
{
    [HttpGet("crash")]
    public IActionResult CrashTest()
    {
        Environment.FailFast("Crashing...");

        return StatusCode(500, "The application has crashed.");
    }
}
