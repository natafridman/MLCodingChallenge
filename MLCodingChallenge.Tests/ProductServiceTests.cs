using Moq;
using MLCodingChallenge.Data;
using MLCodingChallenge.Models;
using MLCodingChallenge.Services;

namespace MLCodingChallenge.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repoMock;
    private readonly ProductService _service;

    // Productos de prueba reutilizables
    private static readonly List<Product> SampleProducts = new()
    {
        new Product
        {
            Id = 1, Name = "Phone A", Description = "Desc A",
            Price = 999.99m, Rating = 4.5, Category = "Smartphones",
            Size = "6.1 inches", Weight = 200, Color = "Black",
            ImageUrl = "https://img.example.com/a.jpg",
            Specifications = new() { { "battery", "4000 mAh" }, { "memory", "8 GB" } }
        },
        new Product
        {
            Id = 2, Name = "Phone B", Description = "Desc B",
            Price = 799.00m, Rating = 4.2, Category = "Smartphones",
            Size = "6.4 inches", Weight = 185, Color = "White",
            ImageUrl = "https://img.example.com/b.jpg",
            Specifications = new() { { "battery", "5000 mAh" }, { "memory", "12 GB" } }
        },
        new Product
        {
            Id = 3, Name = "Laptop C", Description = "Desc C",
            Price = 1499.00m, Rating = 4.8, Category = "Laptops",
            Size = "14 inches", Weight = 1600, Color = "Silver",
            ImageUrl = "https://img.example.com/c.jpg",
            Specifications = new() { { "processor", "i7-13700H" }, { "storage", "512 GB" } }
        }
    };

    public ProductServiceTests()
    {
        _repoMock = new Mock<IProductRepository>();
        _service = new ProductService(_repoMock.Object);
    }

    [Fact]
    public void GetAll_ReturnsAllProducts()
    {
        _repoMock.Setup(r => r.GetAll()).Returns(SampleProducts);

        var result = _service.GetAll();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Compare_WithNoFields_ReturnsAllFieldsForEachProduct()
    {
        _repoMock.Setup(r => r.GetByIds(It.IsAny<IEnumerable<int>>()))
            .Returns(SampleProducts.Take(2).ToList());

        var result = _service.Compare(new List<int> { 1, 2 }, null);

        Assert.Equal(2, result.Products.Count);
        // Sin filtro, tiene que devolver todos los campos del modelo
        Assert.Contains("name", result.Products[0].Keys);
        Assert.Contains("price", result.Products[0].Keys);
        Assert.Contains("specifications", result.Products[0].Keys);
    }

    [Fact]
    public void Compare_WithSpecificFields_ReturnsOnlyThoseFields()
    {
        _repoMock.Setup(r => r.GetByIds(It.IsAny<IEnumerable<int>>()))
            .Returns(SampleProducts.Take(2).ToList());

        var fields = new List<string> { "name", "price" };
        var result = _service.Compare(new List<int> { 1, 2 }, fields);

        // Solo name y price en el resultado
        Assert.Equal(2, result.Fields.Count);
        foreach (var product in result.Products)
        {
            Assert.Equal(2, product.Keys.Count);
            Assert.Contains("name", product.Keys);
            Assert.Contains("price", product.Keys);
        }
    }

    [Fact]
    public void Compare_IgnoresInvalidFieldNames()
    {
        _repoMock.Setup(r => r.GetByIds(It.IsAny<IEnumerable<int>>()))
            .Returns(SampleProducts.Take(2).ToList());

        var fields = new List<string> { "name", "campoInventado", "price" };
        var result = _service.Compare(new List<int> { 1, 2 }, fields);

        // "campoInventado" no existe, se ignora
        Assert.Equal(2, result.Fields.Count);
        Assert.DoesNotContain("campoInventado", result.Fields);
    }

    [Fact]
    public void Compare_WithNoMatchingIds_ReturnsEmptyResponse()
    {
        _repoMock.Setup(r => r.GetByIds(It.IsAny<IEnumerable<int>>()))
            .Returns(new List<Product>());

        var result = _service.Compare(new List<int> { 99, 100 }, null);

        Assert.Empty(result.Products);
    }

    [Fact]
    public void Compare_IncludesSpecificationsWhenRequested()
    {
        _repoMock.Setup(r => r.GetByIds(It.IsAny<IEnumerable<int>>()))
            .Returns(SampleProducts.Take(2).ToList());

        var fields = new List<string> { "name", "specifications" };
        var result = _service.Compare(new List<int> { 1, 2 }, fields);

        // Las specs se devuelven como diccionario
        Assert.True(result.Products[0].ContainsKey("specifications"));
        var specs = result.Products[0]["specifications"] as Dictionary<string, object>;
        Assert.NotNull(specs);
        Assert.Equal("4000 mAh", specs["battery"]);
    }

    [Fact]
    public void Compare_FieldNamesAreCaseInsensitive()
    {
        _repoMock.Setup(r => r.GetByIds(It.IsAny<IEnumerable<int>>()))
            .Returns(SampleProducts.Take(2).ToList());

        // "Name" con mayuscula deberia funcionar igual
        var fields = new List<string> { "Name", "Price" };
        var result = _service.Compare(new List<int> { 1, 2 }, fields);

        Assert.Equal(2, result.Fields.Count);
        Assert.Equal(2, result.Products[0].Keys.Count);
    }

    [Fact]
    public void Compare_ReturnsCorrectValues()
    {
        _repoMock.Setup(r => r.GetByIds(It.IsAny<IEnumerable<int>>()))
            .Returns(new List<Product> { SampleProducts[0] });

        var fields = new List<string> { "name", "price", "rating" };
        var result = _service.Compare(new List<int> { 1 }, fields);

        var product = result.Products[0];
        Assert.Equal("Phone A", product["name"]);
        // Price viene como decimal -> serializado a number
        Assert.Equal(999.99, Convert.ToDouble(product["price"]));
        Assert.Equal(4.5, Convert.ToDouble(product["rating"]));
    }
}
