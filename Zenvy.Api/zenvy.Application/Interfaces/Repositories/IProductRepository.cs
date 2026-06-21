using zenvy.application.DTOs.Products;
namespace zenvy.application.Interfaces.Repositories;
public interface IProductsRepository
{
    Task<int> CreateAsync(CreateProductRequest request);
    Task<IReadOnlyList<int>> BulkCreateAsync(IReadOnlyCollection<CreateProductRequest> requests);
    Task<IReadOnlyList<int>> BulkUpdateAsync(IReadOnlyCollection<BulkUpdateProductItem> requests);
    Task<IEnumerable<ProductResponse>> GetAllAsync(ProductQueryRequest request);
    Task<ProductResponse?> GetByIdAsync(int productMasterId);
    Task<bool> UpdateAsync(int productMasterId, CreateProductRequest request);
    Task<bool> DeleteAsync(int productMasterId);
    Task<bool> UpdateStatusAsync(int productMasterId, bool isActive);
    Task<IEnumerable<ProductDropdownResponse>> GetDropdownAsync();
    Task<IEnumerable<ProductDropdownResponse>> SearchAsync(string? search);
}
