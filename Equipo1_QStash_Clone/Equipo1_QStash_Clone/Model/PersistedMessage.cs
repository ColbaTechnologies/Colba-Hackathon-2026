namespace Equipo1_QStash_Clone.Model;

public class PersistedMessage
{
    public required string Id { get; set; }
    
    public required string QueueId { get; set; }
    
    public required InputMessage InputMessage { get; set; }
    
    public DateTime Timestamp { get; set; }
}