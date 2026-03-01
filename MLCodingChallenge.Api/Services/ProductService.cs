using System.Text.Json;
using MLCodingChallenge.Data;
using MLCodingChallenge.Models;

namespace MLCodingChallenge.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    // Campos base del producto que se pueden usar en la comparacion
    private static readonly HashSet<string> ValidFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "name", "description", "imageUrl", "price",
        "rating", "category", "size", "weight", "color", "specifications"
    };

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public List<Product> GetAll() => _repository.GetAll();

    // Compara productos por IDs, devolviendo solo los campos solicitados (o todos si no se pasan)
    public CompareResponse Compare(List<int> ids, List<string>? fields)
    {
        var products = _repository.GetByIds(ids);
        if (products.Count == 0)
            return new CompareResponse();

        // Si no se especificaron campos, devolver todo
        var selectedFields = (fields is { Count: > 0 })
            ? fields.Where(f => ValidFields.Contains(f)).ToList()
            : ValidFields.ToList();

        var result = new CompareResponse { Fields = selectedFields };

        foreach (var product in products)
        {
            var filtered = FilterFields(product, selectedFields);
            result.Products.Add(filtered);
        }

        return result;
    }

    // Serializa el producto y filtra dejando solo los campos pedidos
    private static Dictionary<string, object> FilterFields(Product product, List<string> fields)
    {
        // Serializo a JSON y luego a diccionario para poder filtrar dinamicamente
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(product, options);
        var all = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, options)!;

        var filtered = new Dictionary<string, object>();

        foreach (var field in fields)
        {
            // Busco la key ignorando mayusculas
            var key = all.Keys.FirstOrDefault(k => k.Equals(field, StringComparison.OrdinalIgnoreCase));
            if (key != null)
            {
                filtered[key] = ConvertJsonElement(all[key]);
            }
        }

        return filtered;
    }

    // Convierte JsonElement a tipos nativos para que el response no tenga ValueKind wrappers
    private static object ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString()!,
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
            JsonValueKind.Array => element.EnumerateArray()
                .Select(ConvertJsonElement).ToList(),
            _ => element.ToString()
        };
    }
}
