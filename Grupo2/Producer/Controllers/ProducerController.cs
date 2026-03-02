using Microsoft.AspNetCore.Mvc;
using Producer.Models;
using Producer.Service;

namespace Producer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProducerController(IMessageService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> StoreRequest([FromBody] CreateMessageRequest request)
    {
        {
            if (string.IsNullOrEmpty(request.Payload) || string.IsNullOrEmpty(request.Url))
            {
                return BadRequest("Payload and Url cannot be null or empty.");
            }
            
            return await service.ProcessMessageAsync(request.Payload, request.Url)
                ? Ok()
                : BadRequest();
        }
    }
}