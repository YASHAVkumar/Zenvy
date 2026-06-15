using zenvy.application.DTOs.Suppliers;

namespace zenvy.application.Interfaces.Services;

public interface ISupplierService
{
    Task<int> CreateSupplierAsync(SupplierRequest request);
    Task<IEnumerable<SupplierResponse>> GetSuppliersAsync();
}
