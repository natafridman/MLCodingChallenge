namespace MLCodingChallenge.Models;

public class CompareResponse
{
    public List<Dictionary<string, object>> Products { get; set; } = new();
    public List<string> Fields { get; set; } = new();
}
