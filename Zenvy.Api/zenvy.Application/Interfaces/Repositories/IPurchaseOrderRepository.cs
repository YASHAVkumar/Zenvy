using zenvy.application.DTOs.PurchaseOrders;

namespace zenvy.application.Interfaces.Repositories;

public interface IPurchaseOrderRepository
{
    Task<long> CreateAsync(PurchaseOrderRequest request);
    Task<IEnumerable<PurchaseOrderResponse>> GetAllAsync();
    Task<PurchaseOrderResponse?> GetByIdAsync(long poId);
}
