namespace Lifeline.Models;

public class Config
{
    public required string Name { set; get; }
    public required string Token { set; get; }
    public required string Target { set; get; }
    public string? Path { set; get; }
}
