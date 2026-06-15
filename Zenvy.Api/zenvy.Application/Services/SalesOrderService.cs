using zenvy.application.DTOs.SalesOrders;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;

namespace zenvy.Application.Services;

public class SalesOrderService(ISalesOrderRepository salesOrderRepository) : ISalesOrderService
{
    public Task<long> CreateSalesOrderAsync(SalesOrderRequest request)
    {
        return salesOrderRepository.CreateAsync(request);
    }

    public Task<IEnumerable<SalesOrderResponse>> GetSalesOrdersAsync()
    {
        return salesOrderRepository.GetAllAsync();
    }

    public Task<SalesOrderResponse?> GetSalesOrderByIdAsync(long orderId)
    {
        return salesOrderRepository.GetByIdAsync(orderId);
    }
}
