using zenvy.application.DTOs.Products;

namespace zenvy.application.Interfaces.Services;
public interface IProductService
{
    Task<CreateProductResponse> CreateProductAsync(CreateProductRequest request);
    Task<BulkCreateProductsResponse> BulkCreateProductsAsync(IReadOnlyCollection<CreateProductRequest> requests);
    Task<BulkUpdateProductsResponse> BulkUpdateProductsAsync(IReadOnlyCollection<BulkUpdateProductItem> requests);
    Task<IEnumerable<ProductResponse>> GetProductsAsync(ProductQueryRequest request);
    Task<ProductResponse?> GetProductByIdAsync(int productMasterId);
    Task<bool> UpdateProductAsync(int productMasterId, CreateProductRequest request);
    Task<bool> DeleteProductAsync(int productMasterId);
    Task<bool> UpdateProductStatusAsync(int productMasterId, bool isActive);
    Task<IEnumerable<ProductDropdownResponse>> GetProductDropdownAsync();
    Task<IEnumerable<ProductDropdownResponse>> SearchProductsAsync(string? search);
}
