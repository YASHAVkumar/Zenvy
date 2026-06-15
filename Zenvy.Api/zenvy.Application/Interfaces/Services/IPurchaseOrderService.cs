using zenvy.application.DTOs.PurchaseOrders;

namespace zenvy.application.Interfaces.Services;

public interface IPurchaseOrderService
{
    Task<long> CreatePurchaseOrderAsync(PurchaseOrderRequest request);
    Task<IEnumerable<PurchaseOrderResponse>> GetPurchaseOrdersAsync();
    Task<PurchaseOrderResponse?> GetPurchaseOrderByIdAsync(long poId);
}
