using System.Net;
using Microsoft.AspNetCore.Mvc;
using UQ.Api.Application;
using UQ.Api.Application.Producer;

namespace UQ.Api.Presentation;

[ApiController]
[Route("/api/v1/caller")]
public class EntryController(IProducer producer) : ControllerBase
{
    [HttpPost]
    [Route("call")]
    public async Task<ActionResult<InitialCallResult>> Update([FromBody] InputEntry entry)
    {
        var valid = Uri.IsWellFormedUriString(entry.DestinationUri, UriKind.RelativeOrAbsolute);
        if (!valid)
        {
            return BadRequest();
        }
        
        var (ok, publicId) = await producer.SavePendingMessage(entry);

        if (!ok)
        {
            return new InitialCallResult(HttpStatusCode.InternalServerError, string.Empty);
        }
        
        var result = new InitialCallResult(HttpStatusCode.OK, publicId);
        return result;
    }
}

public record InitialCallResult(HttpStatusCode ResultCode, string RequestPublicId);