using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

// ── Configuration ─────────────────────────────────────────────────────────────

var baseUrl = args.Length > 0 ? args[0].TrimEnd('/') : "http://localhost:5141";

using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverter() }
};

// ── Boot ──────────────────────────────────────────────────────────────────────

PrintBanner(baseUrl);

// ── REPL ──────────────────────────────────────────────────────────────────────

while (true)
{
    Console.Write("\n> ");
    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(input)) continue;

    var parts   = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
    var command = parts[0].ToLowerInvariant();

    switch (command)
    {
        case "send":
            await HandleSend(http);
            break;

        case "status":
            var requestId = parts.Length > 1 ? parts[1].Trim() : PromptLine("Request ID");
            await HandleStatus(http, requestId);
            break;

        case "help":
            PrintHelp();
            break;

        case "exit":
        case "quit":
            Console.WriteLine("Bye.");
            return;

        default:
            Warn($"Unknown command '{command}'. Type 'help' for available commands.");
            break;
    }
}

// ── Handlers ──────────────────────────────────────────────────────────────────


async Task HandleSend(HttpClient client)
{
    var targetUrl = PromptLine("Target URL");
    if (string.IsNullOrWhiteSpace(targetUrl)) { Warn("Target URL cannot be empty."); return; }

    var rawPayload = PromptLine("Payload (JSON)");
    if (string.IsNullOrWhiteSpace(rawPayload)) { Warn("Payload cannot be empty."); return; }

    JsonElement payload;
    try
    {
        payload = JsonDocument.Parse(rawPayload).RootElement;
    }
    catch (JsonException)
    {
        Warn("Invalid JSON payload.");
        return;
    }

    try
    {
        var response = await client.PostAsJsonAsync("/messages", new { targetUrl, payload });

        if (!response.IsSuccessStatusCode)
        {
            Warn($"Server returned {(int)response.StatusCode} {response.ReasonPhrase}");
            var body = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(body)) Console.WriteLine($"  {body}");
            return;
        }

        var result = await response.Content.ReadFromJsonAsync<SendResponse>();
        Ok("Request accepted.");
        Console.WriteLine($"  Request ID : {result!.RequestId}");
        Console.WriteLine($"  Status URL : {client.BaseAddress}messages/{result.RequestId}");
    }
    catch (HttpRequestException ex)
    {
        Warn($"Could not reach server: {ex.Message}");
    }
}

async Task HandleStatus(HttpClient client, string requestId)
{
    if (string.IsNullOrWhiteSpace(requestId)) { Warn("Request ID cannot be empty."); return; }

    try
    {
        var response = await client.GetAsync($"/messages/{requestId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Warn($"No request found with ID '{requestId}'.");
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            Warn($"Server returned {(int)response.StatusCode} {response.ReasonPhrase}");
            return;
        }

        var status = await response.Content.ReadFromJsonAsync<StatusResponse>(jsonOptions);

        Console.WriteLine();
        Console.WriteLine($"  Request ID  : {status!.RequestId}");
        Console.WriteLine($"  Target URL  : {status.TargetUrl}");
        Console.WriteLine($"  State       : {StateLabel(status.State)}");
        Console.WriteLine($"  Retry Count : {status.RetryCount}");
        Console.WriteLine($"  Received At : {status.ReceivedAt:u}");
        Console.WriteLine($"  Delivered At: {(status.DeliveredAt.HasValue ? status.DeliveredAt.Value.ToString("u") : "—")}");
    }
    catch (HttpRequestException ex)
    {
        Warn($"Could not reach server: {ex.Message}");
    }
}

// ── Helpers ───────────────────────────────────────────────────────────────────

static string PromptLine(string label)
{
    Console.Write($"  {label}: ");
    return Console.ReadLine()?.Trim() ?? string.Empty;
}

static string StateLabel(string state) => state switch
{
    "Pending"   => "⏳ Pending",
    "Retrying"  => "🔄 Retrying",
    "Erroneous" => "❌ Erroneous",
    "Delivered" => "✅ Delivered",
    _           => state
};

static void Ok(string msg)   => WriteColored($"✓ {msg}", ConsoleColor.Green);
static void Warn(string msg) => WriteColored($"⚠ {msg}", ConsoleColor.Yellow);

static void WriteColored(string msg, ConsoleColor color)
{
    var prev = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.WriteLine(msg);
    Console.ForegroundColor = prev;
}

static void PrintHelp()
{
    Console.WriteLine();
    Console.WriteLine("  Commands:");
    Console.WriteLine("    send              – Send a new message (prompts for URL and payload)");
    Console.WriteLine("    status <id>       – Get the delivery status of a request");
    Console.WriteLine("    help              – Show this help");
    Console.WriteLine("    exit | quit       – Quit the client");
}

static void PrintBanner(string baseUrl)
{
    Console.WriteLine("╔══════════════════════════════════════════╗");
    Console.WriteLine("║     ActorBaseMessaging  Client           ║");
    Console.WriteLine("╚══════════════════════════════════════════╝");
    Console.WriteLine($"  Server : {baseUrl}");
    Console.WriteLine("  Type 'help' for available commands.");
}

// ── Models ────────────────────────────────────────────────────────────────────

record SendResponse(string RequestId);

record StatusResponse(
    string    RequestId,
    string    TargetUrl,
    string    State,
    int       RetryCount,
    DateTime  ReceivedAt,
    DateTime? DeliveredAt);
