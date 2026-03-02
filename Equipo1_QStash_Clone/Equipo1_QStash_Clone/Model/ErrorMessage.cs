namespace Equipo1_QStash_Clone.Model;

public class ErrorMessage
{
    public required string Id { get; set; }
    public string? Error { get; set; }
    public required PersistedMessage PersistedMessage { get; set; }
}