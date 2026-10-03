using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using zenvy.Application.DTOs.Category;
using zenvy.Application.Interfaces.Services;

namespace zenvy.tests;

public sealed class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _service = new();
    private CategoriesController CreateSubject() => new(_service.Object);

    [Fact]
    public async Task Create_returns_created_category_identifier()
    {
        var request = new CreateCategoryRequest { CategoryName = "Accessories" };
        _service.Setup(x => x.CreateAsync(request)).ReturnsAsync(11);

        var result = await CreateSubject().Create(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(11L, ok.Value!.GetType().GetProperty("CategoryId")!.GetValue(ok.Value));
    }

    [Fact]
    public async Task GetById_returns_not_found_when_service_has_no_category()
    {
        _service.Setup(x => x.GetByIdAsync(404)).ReturnsAsync((CategoryResponse?)null);

        var result = await CreateSubject().GetById(404);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_returns_category_from_service()
    {
        var category = new CategoryResponse { CategoryId = 11, CategoryName = "Accessories", IsActive = true };
        _service.Setup(x => x.GetByIdAsync(11)).ReturnsAsync(category);

        var result = await CreateSubject().GetById(11);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(category, ok.Value);
    }

    [Fact]
    public async Task Update_and_delete_return_service_success_values()
    {
        var request = new UpdateCategoryRequest { CategoryId = 11, CategoryName = "Accessories" };
        _service.Setup(x => x.UpdateAsync(request)).ReturnsAsync(true);
        _service.Setup(x => x.DeleteAsync(11)).ReturnsAsync(false);

        var updated = Assert.IsType<OkObjectResult>(await CreateSubject().Update(request));
        var deleted = Assert.IsType<OkObjectResult>(await CreateSubject().Delete(11));

        Assert.Equal(true, updated.Value!.GetType().GetProperty("Success")!.GetValue(updated.Value));
        Assert.Equal(false, deleted.Value!.GetType().GetProperty("Success")!.GetValue(deleted.Value));
    }
}
