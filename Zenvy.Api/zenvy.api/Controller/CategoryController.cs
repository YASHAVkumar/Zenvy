using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.Application.DTOs.Category;
using zenvy.Application.Interfaces.Services;

[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    private readonly ICategoryService categoryService = categoryService;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        long categoryId = await categoryService.CreateAsync(request);
        return Ok(new { CategoryId = categoryId });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateCategoryRequest request)
    {
        bool success = await categoryService.UpdateAsync(request);
        return Ok(new { Success = success });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        bool success = await categoryService.DeleteAsync(id);
        return Ok(new { Success = success });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var category = await categoryService.GetByIdAsync(id);
        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await categoryService.GetAllAsync();
        return Ok(categories);
    }
}
