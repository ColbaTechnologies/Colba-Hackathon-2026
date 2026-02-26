namespace Equipo1_QStash_Clone.Model;

public class InputMessage
{
    public required string Url { get; set; }
    
    public required string Method { get; set; }
    
    public required string Body { get; set; }
    
    public  Dictionary<string, string>? Headers { get; set; }
    
    public string? Callback { get; set; }
    
    public required string QueueId { get; set; }

    public int Retries { get; set; } = 3;
}