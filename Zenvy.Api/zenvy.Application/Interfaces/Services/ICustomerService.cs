using zenvy.application.DTOs.Customers;

namespace zenvy.application.Interfaces.Services;

public interface ICustomerService
{
    Task<int> CreateCustomerAsync(CustomerRequest request);
    Task<IEnumerable<CustomerResponse>> GetCustomersAsync();
}
