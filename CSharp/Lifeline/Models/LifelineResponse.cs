namespace Lifeline.Models;

public class LifelineResponse
{
    public required string Name { set; get; }
    public string? Id { set; get; }
    public required string LastInsert { set; get; }
    public string? Data { set; get; }
}
