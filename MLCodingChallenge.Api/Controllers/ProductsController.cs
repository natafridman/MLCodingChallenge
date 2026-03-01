using Microsoft.AspNetCore.Mvc;
using MLCodingChallenge.Services;

namespace MLCodingChallenge.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET api/products
    [HttpGet]
    public IActionResult GetAll()
    {
        var products = _productService.GetAll();
        return Ok(products);
    }

    // GET api/products/compare?ids=1,2,3&fields=name,price,rating
    [HttpGet("compare")]
    public IActionResult Compare([FromQuery] string ids, [FromQuery] string? fields = null)
    {
        if (string.IsNullOrWhiteSpace(ids))
            return BadRequest(new { error = "The 'ids' parameter is required. Example: ?ids=1,2,3" });

        // Parseo los IDs separados por coma
        var parsedIds = new List<int>();
        foreach (var raw in ids.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!int.TryParse(raw.Trim(), out var id))
                return BadRequest(new { error = $"Invalid id: '{raw.Trim()}'. Must be an integer." });

            parsedIds.Add(id);
        }

        if (parsedIds.Count < 2)
            return BadRequest(new { error = "At least 2 product ids are required for a comparison." });

        // Parseo los fields (opcional)
        var parsedFields = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToList();

        var result = _productService.Compare(parsedIds, parsedFields);

        if (result.Products.Count == 0)
            return NotFound(new { error = "No products found for the given ids." });

        return Ok(result);
    }
}
