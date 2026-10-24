namespace Lifeline.Models;

public class LifelineResponse
{
    public required string Name { set; get; }
    public required string Id { set; get; }
    public required DateTime LastInsert { set; get; }
    public string? Data { set; get; }
}
