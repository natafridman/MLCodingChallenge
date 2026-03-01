using System.Text.Json;
using MLCodingChallenge.Models;

namespace MLCodingChallenge.Data;

public class JsonProductRepository : IProductRepository
{
    private readonly List<Product> _products;

    public JsonProductRepository(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "products.json");
        var json = File.ReadAllText(path);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _products = JsonSerializer.Deserialize<List<Product>>(json, options) ?? new List<Product>();
    }

    public List<Product> GetAll() => _products;

    public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

    public List<Product> GetByIds(IEnumerable<int> ids)
    {
        var idSet = ids.ToHashSet();
        return _products.Where(p => idSet.Contains(p.Id)).ToList();
    }
}
