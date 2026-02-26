using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Persistence;
using WebApplication1.Processing;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/messages")]
public sealed class MessagesController : ControllerBase
{
    private readonly IMessageStore _store;
    private readonly IMessageQueue _queue;
    private readonly ILogger<MessagesController> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MessagesController(
        IMessageStore store,
        IMessageQueue queue,
        ILogger<MessagesController> logger)
    {
        _store = store;
        _queue = queue;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    [HttpPost]
    public async Task<IActionResult> Enqueue([FromBody] EnqueueRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DestinationUrl))
        {
            return BadRequest("destinationUrl is required.");
        }

        var headers = request.Headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        string? serializedPayload = null;
        if (request.Payload != null)
        {
            serializedPayload = JsonSerializer.Serialize(request.Payload, _jsonOptions);
        }
        
        var record = await _store.EnqueueAsync(
            request.DestinationUrl,
            request.ClientMessageId,
            serializedPayload,
            headers,
            request.TenantId,
            request.CallbackUrl,
            request.DeliverAtUtc,
            cancellationToken);

        if (record.Status == MessageStatus.Pending && record.NextAttemptAtUtc <= DateTimeOffset.UtcNow)
        {
            _queue.Enqueue(record.Id);
        }

        _logger.LogInformation("Enqueued message {MessageId} to {DestinationUrl}", record.Id, record.DestinationUrl);

        return Accepted(new
        {
            id = record.Id,
            status = record.Status.ToString(),
            createdAtUtc = record.CreatedAtUtc,
            nextAttemptAtUtc = record.NextAttemptAtUtc,
            tenantId = record.TenantId
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MessageSummary>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var record = await _store.TryGetByIdAsync(id, cancellationToken);
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
    
    /*

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageSummary>>> List([FromQuery] string? tenantId, CancellationToken cancellationToken)
    {
        var records = await _store.GetActiveMessagesSnapshotAsync(tenantId, cancellationToken);

        var summaries = records
            .Select(record => new MessageSummary
            {
                MessageId = record.Id,
                DestinationUrl = record.DestinationUrl,
                Status = record.Status,
                AttemptCount = record.AttemptCount,
                CreatedAtUtc = record.CreatedAtUtc,
                LastAttemptAtUtc = record.LastAttemptAtUtc,
                NextAttemptAtUtc = record.NextAttemptAtUtc,
                LastError = record.LastError,
                TenantId = record.TenantId
            })
            .ToList();

        return Ok(summaries);
    }
    
    */
}

