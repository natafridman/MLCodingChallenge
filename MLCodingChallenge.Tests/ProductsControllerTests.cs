using Microsoft.AspNetCore.Mvc;
using Moq;
using MLCodingChallenge.Controllers;
using MLCodingChallenge.Models;
using MLCodingChallenge.Services;

namespace MLCodingChallenge.Tests;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _serviceMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _serviceMock = new Mock<IProductService>();
        _controller = new ProductsController(_serviceMock.Object);
    }

    [Fact]
    public void GetAll_ReturnsOkWithProducts()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1" },
            new() { Id = 2, Name = "Product 2" }
        };
        _serviceMock.Setup(s => s.GetAll()).Returns(products);

        var result = _controller.GetAll() as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        var returned = result.Value as List<Product>;
        Assert.Equal(2, returned!.Count);
    }

    [Fact]
    public void Compare_WithEmptyIds_ReturnsBadRequest()
    {
        var result = _controller.Compare("", null) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void Compare_WithNonNumericId_ReturnsBadRequest()
    {
        var result = _controller.Compare("1,abc,3", null) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void Compare_WithSingleId_ReturnsBadRequest()
    {
        // Se necesitan al menos 2 productos para comparar
        var result = _controller.Compare("1", null) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void Compare_WhenNoProductsFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.Compare(It.IsAny<List<int>>(), It.IsAny<List<string>?>()))
            .Returns(new CompareResponse());

        var result = _controller.Compare("99,100", null) as NotFoundObjectResult;

        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void Compare_WithValidIds_ReturnsOk()
    {
        var response = new CompareResponse
        {
            Fields = new List<string> { "name", "price" },
            Products = new List<Dictionary<string, object>>
            {
                new() { { "name", "Phone A" }, { "price", 999.99 } },
                new() { { "name", "Phone B" }, { "price", 799.00 } }
            }
        };
        _serviceMock.Setup(s => s.Compare(It.IsAny<List<int>>(), It.IsAny<List<string>?>()))
            .Returns(response);

        var result = _controller.Compare("1,2", "name,price") as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        var body = result.Value as CompareResponse;
        Assert.Equal(2, body!.Products.Count);
    }

    [Fact]
    public void Compare_WithoutFields_PassesNullToService()
    {
        _serviceMock.Setup(s => s.Compare(It.IsAny<List<int>>(), null))
            .Returns(new CompareResponse
            {
                Products = new List<Dictionary<string, object>> { new() { { "id", 1 } } },
                Fields = new List<string> { "id" }
            });

        var result = _controller.Compare("1,2", null) as OkObjectResult;

        Assert.NotNull(result);
        // Verifico que se llamo al service con fields null
        _serviceMock.Verify(s => s.Compare(It.IsAny<List<int>>(), null), Times.Once);
    }

    [Fact]
    public void Compare_ParsesIdsCorrectly()
    {
        List<int>? capturedIds = null;
        _serviceMock.Setup(s => s.Compare(It.IsAny<List<int>>(), It.IsAny<List<string>?>()))
            .Callback<List<int>, List<string>?>((ids, _) => capturedIds = ids)
            .Returns(new CompareResponse
            {
                Products = new List<Dictionary<string, object>> { new() { { "id", 1 } } },
                Fields = new List<string> { "id" }
            });

        _controller.Compare("1, 3, 5", null);

        Assert.NotNull(capturedIds);
        Assert.Equal(new List<int> { 1, 3, 5 }, capturedIds);
    }
}
