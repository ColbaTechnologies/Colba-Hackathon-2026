using Microsoft.AspNetCore.Mvc;
using UQ.Api.Application;

namespace UQ.Api.Presentation;

[ApiController]
[Route("/api/v1/caller")]
public class EntryController(IProducer producer) : ControllerBase
{
    [HttpPost]
    [Route("call")]
    public async Task<ActionResult<InitialCallResult>> Update([FromBody] InputEntry entry)
    {
        // call producer
        
        var ok = await producer.SavePendingMessage(entry);
        // producer returns ok & public id of the request
        
        // return 200 & public id
        var result = new InitialCallResult(Guid.NewGuid().ToString());
        return result;
    }
}

public record InitialCallResult(string RequestPublicId);