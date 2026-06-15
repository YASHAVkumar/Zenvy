using zenvy.application.DTOs.SalesOrders;

namespace zenvy.application.Interfaces.Repositories;

public interface ISalesOrderRepository
{
    Task<long> CreateAsync(SalesOrderRequest request);
    Task<IEnumerable<SalesOrderResponse>> GetAllAsync();
    Task<SalesOrderResponse?> GetByIdAsync(long orderId);
}
