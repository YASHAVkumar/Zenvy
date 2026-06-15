using zenvy.application.DTOs.Suppliers;

namespace zenvy.application.Interfaces.Repositories;

public interface ISupplierRepository
{
    Task<int> CreateAsync(SupplierRequest request);
    Task<IEnumerable<SupplierResponse>> GetAllAsync();
}
