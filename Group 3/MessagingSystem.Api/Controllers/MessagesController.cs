using System.Text.Json;
using MessagingSystem.Application.Dtos;
using MessagingSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MessagingSystem.Controllers;

[ApiController]
[Route("api/messages")]
public sealed class MessagesController(
    IMessageStore store,
    ILogger<MessagesController> logger)
    : ControllerBase
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    [HttpPost]
    public async Task<IActionResult> Enqueue(
        [FromBody] EnqueueRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await EnqueueInternalAsync(request, ct);

            logger.LogInformation(
                "Enqueued message {MessageId} to {DestinationUrl}",
                result.Id,
                request.DestinationUrl);

            return Accepted(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("batch")]
    public async Task<IActionResult> EnqueueBatch(
        [FromBody] BatchEnqueueRequest request,
        CancellationToken ct)
    {
        if (request.Items == null || request.Items.Count == 0)
            return BadRequest("Batch cannot be empty.");

        var results = new List<EnqueueResult>();

        foreach (var item in request.Items)
        {
            try
            {
                var result = await EnqueueInternalAsync(item, ct);
                results.Add(result);
            }
            catch
            {
               
            }
        }

        return Accepted(results);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MessageSummary>> GetById(string id, CancellationToken cancellationToken)
    {
        var record = await store.TryGetByIdAsync(id, cancellationToken);
        if (record == null)
        {
            return NotFound();
        }

        var summary = new MessageSummary
        {
            MessageId = record.Id,
            DestinationUrl = record.DestinationUrl,
            Status = record.Status,
            AttemptCount = record.AttemptCount,
            CreatedAtUtc = record.CreatedAtUtc,
            LastAttemptAtUtc = record.LastAttemptAtUtc,
            LastError = record.LastError,
            TenantId = record.TenantId
        };

        return Ok(summary);
    }

    [HttpGet("{id}/operations")]
    public async Task<IActionResult> GetOperations(
        string id,
        CancellationToken ct)
    {
        var result = await store.GetMessageOperationsAsync(id, ct);

        if (result.Count == 0)
            return NotFound();

        return Ok(result);
    }

    private async Task<EnqueueResult> EnqueueInternalAsync(
        EnqueueRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DestinationUrl))
            throw new ArgumentException("destinationUrl is required.");

        var headers = request.Headers 
                      ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var serializedPayload = request.Payload != null
            ? JsonSerializer.Serialize(request.Payload, _jsonOptions)
            : null;

        var record = await store.EnqueuePendingMessage(
            request.DestinationUrl,
            serializedPayload,
            headers,
            request.TenantId,
            request.CallbackUrl,
            ct);

        return new EnqueueResult
        {
            Id = record.ClientMessageId,
            Status = record.Status.ToString(),
            CreatedAtUtc = record.CreatedAtUtc,
            NextAttemptAtUtc = record.NextAttemptAtUtc,
            TenantId = record.TenantId
        };
    }
}

