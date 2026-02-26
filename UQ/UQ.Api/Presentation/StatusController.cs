using Microsoft.AspNetCore.Mvc;
using UQ.Api.Application.Repositories;

namespace UQ.Api.Presentation;

[ApiController]
[Route("/api/v1/requestStatus")]
public class StatusController(IMessageRepository repository) : ControllerBase
{
    [HttpGet]
    [Route("{publicId}")]
    public async Task<ActionResult<string>> GetStatus(string publicId)
    {
        var state = await repository.GetMessageState(publicId);
        if (state is null) return NotFound();
        return state.ToString();
    }
}