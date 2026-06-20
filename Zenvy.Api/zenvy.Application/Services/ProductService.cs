using zenvy.application.DTOs.Products;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;

namespace zenvy.Application.Services;

public class ProductService(IProductsRepository productRepository) : IProductService
{
    public async Task<CreateProductResponse> CreateProductAsync(CreateProductRequest request)
    {
        var productId = await productRepository.CreateAsync(request);
        return new CreateProductResponse
        {
            ProductId = productId,
            Message = "Product created successfully"
        };
    }

    public async Task<BulkCreateProductsResponse> BulkCreateProductsAsync(IReadOnlyCollection<CreateProductRequest> requests)
    {
        var ids = await productRepository.BulkCreateAsync(requests);
        return new BulkCreateProductsResponse { CreatedCount = ids.Count, ProductMasterIds = [.. ids] };
    }

    public async Task<BulkUpdateProductsResponse> BulkUpdateProductsAsync(IReadOnlyCollection<BulkUpdateProductItem> requests)
    {
        var ids = await productRepository.BulkUpdateAsync(requests);
        return new BulkUpdateProductsResponse { UpdatedCount = ids.Count, ProductMasterIds = [.. ids] };
    }

    public Task<IEnumerable<ProductResponse>> GetProductsAsync(ProductQueryRequest request)
    {
        return productRepository.GetAllAsync(request);
    }

    public Task<ProductResponse?> GetProductByIdAsync(int productMasterId)
    {
        return productRepository.GetByIdAsync(productMasterId);
    }

    public Task<bool> UpdateProductAsync(int productMasterId, CreateProductRequest request)
    {
        return productRepository.UpdateAsync(productMasterId, request);
    }

    public Task<bool> DeleteProductAsync(int productMasterId)
    {
        return productRepository.DeleteAsync(productMasterId);
    }

    public Task<bool> UpdateProductStatusAsync(int productMasterId, bool isActive)
    {
        return productRepository.UpdateStatusAsync(productMasterId, isActive);
    }

    public Task<IEnumerable<ProductDropdownResponse>> GetProductDropdownAsync()
    {
        return productRepository.GetDropdownAsync();
    }

    public Task<IEnumerable<ProductDropdownResponse>> SearchProductsAsync(string? search)
    {
        return productRepository.SearchAsync(search);
    }
}
