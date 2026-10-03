using Moq;
using Xunit;
using zenvy.application.DTOs.Brands;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Services;

namespace zenvy.tests;

public sealed class BrandServiceTests
{
    private readonly Mock<IBrandRepository> _repository = new();
    private BrandService CreateSubject() => new(_repository.Object);

    [Fact]
    public async Task CreateBrandAsync_returns_repository_id()
    {
        var request = new BrandRequest { Name = "Zenvy", Description = "Main brand", Status = true };
        _repository.Setup(x => x.CreateAsync(request)).ReturnsAsync(42);

        var result = await CreateSubject().CreateBrandAsync(request);

        Assert.Equal(42, result);
        _repository.Verify(x => x.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task GetBrandByIdAsync_maps_repository_response()
    {
        _repository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(new BrandResponse
        {
            BrandId = 7, Name = "North", Description = "Winter collection", Status = true
        });

        var result = await CreateSubject().GetBrandByIdAsync(7);

        Assert.Equal(7, result.BrandId);
        Assert.Equal("North", result.Name);
        Assert.Equal("Winter collection", result.Description);
        Assert.True(result.Status);
    }

    [Fact]
    public async Task GetBrandByIdAsync_throws_when_brand_does_not_exist()
    {
        _repository.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((BrandResponse?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => CreateSubject().GetBrandByIdAsync(99));
    }

    [Fact]
    public async Task GetAllBrandsAsync_maps_each_brand()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync(new[]
        {
            new BrandResponse { BrandId = 1, Name = "One", Status = true },
            new BrandResponse { BrandId = 2, Name = "Two", Description = "Second" }
        });

        var result = (await CreateSubject().GetAllBrandsAsync()).ToList();

        Assert.Collection(result,
            brand => { Assert.Equal(1, brand.BrandId); Assert.Equal("One", brand.Name); Assert.True(brand.Status); },
            brand => { Assert.Equal(2, brand.BrandId); Assert.Equal("Second", brand.Description); });
    }

    [Fact]
    public async Task UpdateBrandAsync_checks_existence_and_passes_a_copy_to_repository()
    {
        var request = new UpdateBrandRequest { Name = "Updated", Description = "New", Status = true };
        _repository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(new BrandResponse { BrandId = 5 });
        _repository.Setup(x => x.UpdateAsync(5, It.Is<UpdateBrandRequest>(update =>
            update.Name == request.Name && update.Description == request.Description && update.Status == request.Status)))
            .ReturnsAsync(true);

        var result = await CreateSubject().UpdateBrandAsync(5, request);

        Assert.True(result);
        _repository.Verify(x => x.GetByIdAsync(5), Times.Once);
        _repository.Verify(x => x.UpdateAsync(5, It.Is<UpdateBrandRequest>(update => !ReferenceEquals(update, request))), Times.Once);
    }

    [Fact]
    public async Task DeleteBrandAsync_does_not_delete_missing_brand()
    {
        _repository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync((BrandResponse?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => CreateSubject().DeleteBrandAsync(5));

        _repository.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
