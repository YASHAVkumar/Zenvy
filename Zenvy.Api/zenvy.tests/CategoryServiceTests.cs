using Moq;
using Xunit;
using zenvy.Application.DTOs.Category;
using zenvy.Application.Interfaces.Repositories;
using zenvy.Application.Services;

namespace zenvy.tests;

public sealed class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repository = new();
    private CategoryService CreateSubject() => new(_repository.Object);

    [Fact]
    public async Task CreateAsync_returns_the_repository_identifier()
    {
        var request = new CreateCategoryRequest { CategoryName = "Footwear", Description = "Shoes" };
        _repository.Setup(x => x.CreateAsync(request)).ReturnsAsync(21);

        var result = await CreateSubject().CreateAsync(request);

        Assert.Equal(21, result);
        _repository.Verify(x => x.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_and_deleteAsync_return_repository_outcomes()
    {
        var update = new UpdateCategoryRequest { CategoryId = 21, CategoryName = "New footwear" };
        _repository.Setup(x => x.UpdateAsync(update)).ReturnsAsync(true);
        _repository.Setup(x => x.DeleteAsync(21)).ReturnsAsync(false);

        Assert.True(await CreateSubject().UpdateAsync(update));
        Assert.False(await CreateSubject().DeleteAsync(21));
        _repository.Verify(x => x.UpdateAsync(update), Times.Once);
        _repository.Verify(x => x.DeleteAsync(21), Times.Once);
    }

    [Fact]
    public async Task Query_methods_return_the_exact_repository_results()
    {
        var expected = new CategoryResponse { CategoryId = 5, CategoryName = "Accessories", IsActive = true };
        _repository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(expected);
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync(new[] { expected });

        var byId = await CreateSubject().GetByIdAsync(5);
        var all = await CreateSubject().GetAllAsync();

        Assert.Same(expected, byId);
        Assert.Single(all, expected);
    }
}
