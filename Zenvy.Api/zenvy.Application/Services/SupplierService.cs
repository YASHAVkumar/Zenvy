using zenvy.application.DTOs.Suppliers;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;

namespace zenvy.Application.Services;

public class SupplierService(ISupplierRepository supplierRepository) : ISupplierService
{
    public Task<int> CreateSupplierAsync(SupplierRequest request)
    {
        return supplierRepository.CreateAsync(request);
    }

    public Task<IEnumerable<SupplierResponse>> GetSuppliersAsync()
    {
        return supplierRepository.GetAllAsync();
    }
}
