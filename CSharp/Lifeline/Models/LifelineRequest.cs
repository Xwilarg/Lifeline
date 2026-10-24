namespace Lifeline.Models;

public class LifelineRequest
{
    public required string Id { set; get; }
    public required string Token { set; get; }
    public string? Data { set; get; }
}
