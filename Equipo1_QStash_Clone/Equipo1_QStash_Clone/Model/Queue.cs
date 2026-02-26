namespace Equipo1_QStash_Clone.Model;

public class Queue
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    
    public string? DeathLetterQueueId { get; set; }
    
    public string? DeathLetterQueueName { get; set; } 
 
    public int Retries { get; set; } = 3;
}
