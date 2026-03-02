using Microsoft.AspNetCore.Mvc;

namespace Consumer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsumerController : ControllerBase
{
    private static readonly Random Random = new();
    private const int ErrorThreshold = 75;
    private const string EmptyPayloadMessage = "Payload cannot be null or empty.";

    [HttpPost]
    public IActionResult ReceiveMessage([FromBody] string? request)
    {
        if (string.IsNullOrWhiteSpace(request))
        {
            return BadRequest(EmptyPayloadMessage);
        }

        if (Random.Next(1, 100) > ErrorThreshold)
        {
            return BadRequest();
        }

        return Ok();
    }
}