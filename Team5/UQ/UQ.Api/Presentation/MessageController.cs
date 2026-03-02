using System.Net;
using Microsoft.AspNetCore.Mvc;
using UQ.Api.Application.Producer;
using UQ.Api.Presentation.Dtos;

namespace UQ.Api.Presentation;

[ApiController]
[Route("/api/v1/sender")]
public class MessageController(IProducer producer) : ControllerBase
{
    [HttpPost]
    [Route("send")]
    public async Task<ActionResult<InitialCallResult>> Send([FromBody] MessageInput entry)
    {
        var valid = Uri.IsWellFormedUriString(entry.DestinationUrl, UriKind.RelativeOrAbsolute);
        if (!valid) return BadRequest();

        var (ok, publicId) = await producer.SavePendingMessage(entry);

        if (!ok) return new InitialCallResult(HttpStatusCode.InternalServerError, string.Empty);

        var result = new InitialCallResult(HttpStatusCode.OK, publicId);
        return result;
    }
}

public record InitialCallResult(HttpStatusCode ResultCode, string RequestPublicId);