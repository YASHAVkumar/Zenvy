using zenvy.application.DTOs.Customers;

namespace zenvy.application.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<int> CreateAsync(CustomerRequest request);
    Task<IEnumerable<CustomerResponse>> GetAllAsync();
}
