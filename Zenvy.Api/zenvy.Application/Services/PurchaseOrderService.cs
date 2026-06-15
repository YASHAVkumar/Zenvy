using zenvy.application.DTOs.PurchaseOrders;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;

namespace zenvy.Application.Services;

public class PurchaseOrderService(IPurchaseOrderRepository purchaseOrderRepository) : IPurchaseOrderService
{
    public Task<long> CreatePurchaseOrderAsync(PurchaseOrderRequest request)
    {
        return purchaseOrderRepository.CreateAsync(request);
    }

    public Task<IEnumerable<PurchaseOrderResponse>> GetPurchaseOrdersAsync()
    {
        return purchaseOrderRepository.GetAllAsync();
    }

    public Task<PurchaseOrderResponse?> GetPurchaseOrderByIdAsync(long poId)
    {
        return purchaseOrderRepository.GetByIdAsync(poId);
    }
}
