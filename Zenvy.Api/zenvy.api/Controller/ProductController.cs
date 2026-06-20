using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Products;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;
[Authorize]
[Route("api/products")]
[ApiController]
public class ProductController(IProductService productService) : ControllerBase
{
    private readonly IProductService _productService = productService;

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var response = await _productService.CreateProductAsync(request);
        return Ok(response);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkCreateProducts([FromBody] BulkCreateProductsRequest request)
    {
        if (request.Products.Count == 0) return BadRequest("At least one product is required.");
        if (request.Products.Count > 100) return BadRequest("A maximum of 100 products is allowed per request.");
        if (request.Products.Any(x => string.IsNullOrWhiteSpace(x.ProductMaster.ProductCode) || string.IsNullOrWhiteSpace(x.ProductMaster.ProductName)))
            return BadRequest("ProductCode and ProductName are required for every product.");
        return Ok(await _productService.BulkCreateProductsAsync(request.Products));
    }

    [HttpPut("bulk")]
    public async Task<IActionResult> BulkUpdateProducts([FromBody] BulkUpdateProductsRequest request)
    {
        if (request.Products.Count == 0) return BadRequest("At least one product is required.");
        if (request.Products.Count > 100) return BadRequest("A maximum of 100 products is allowed per request.");
        if (request.Products.Any(x => x.ProductMasterId <= 0 || string.IsNullOrWhiteSpace(x.Product.ProductMaster.ProductName)))
            return BadRequest("A valid ProductMasterId and ProductName are required for every product.");
        if (request.Products.Select(x => x.ProductMasterId).Distinct().Count() != request.Products.Count)
            return BadRequest("Duplicate ProductMasterId values are not allowed.");
        return Ok(await _productService.BulkUpdateProductsAsync(request.Products));
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductQueryRequest request)
    {
        var response = await _productService.GetProductsAsync(request);
        return Ok(response);
    }

    [HttpGet("{productMasterId:int}")]
    public async Task<IActionResult> GetProductById(int productMasterId)
    {
        var response = await _productService.GetProductByIdAsync(productMasterId);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpPut("{productMasterId:int}")]
    public async Task<IActionResult> UpdateProduct(int productMasterId, [FromBody] CreateProductRequest request)
    {
        var success = await _productService.UpdateProductAsync(productMasterId, request);
        return Ok(new { Success = success });
    }

    [HttpDelete("{productMasterId:int}")]
    public async Task<IActionResult> DeleteProduct(int productMasterId)
    {
        var success = await _productService.DeleteProductAsync(productMasterId);
        return Ok(new { Success = success });
    }

    [HttpPatch("{productMasterId:int}/status")]
    public async Task<IActionResult> UpdateProductStatus(int productMasterId, [FromBody] UpdateProductStatusRequest request)
    {
        var success = await _productService.UpdateProductStatusAsync(productMasterId, request.IsActive);
        return Ok(new { Success = success });
    }

    [HttpGet("dropdown")]
    public async Task<IActionResult> GetProductDropdown()
    {
        var response = await _productService.GetProductDropdownAsync();
        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string? search)
    {
        var response = await _productService.SearchProductsAsync(search);
        return Ok(response);
    }
}
