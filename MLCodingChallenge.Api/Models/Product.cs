namespace MLCodingChallenge.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double Rating { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public double Weight { get; set; }
    public string Color { get; set; } = string.Empty;

    // Cada producto puede tener specs propias segun su categoria
    // (ej: un celular tiene bateria, camara, etc.)
    public Dictionary<string, string> Specifications { get; set; } = new();
}
