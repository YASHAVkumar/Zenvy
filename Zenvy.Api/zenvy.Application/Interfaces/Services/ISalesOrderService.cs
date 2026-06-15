using zenvy.application.DTOs.SalesOrders;

namespace zenvy.application.Interfaces.Services;

public interface ISalesOrderService
{
    Task<long> CreateSalesOrderAsync(SalesOrderRequest request);
    Task<IEnumerable<SalesOrderResponse>> GetSalesOrdersAsync();
    Task<SalesOrderResponse?> GetSalesOrderByIdAsync(long orderId);
}
