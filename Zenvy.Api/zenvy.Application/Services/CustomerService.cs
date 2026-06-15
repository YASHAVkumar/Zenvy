using zenvy.application.DTOs.Customers;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;

namespace zenvy.Application.Services;

public class CustomerService(ICustomerRepository customerRepository) : ICustomerService
{
    public Task<int> CreateCustomerAsync(CustomerRequest request)
    {
        return customerRepository.CreateAsync(request);
    }

    public Task<IEnumerable<CustomerResponse>> GetCustomersAsync()
    {
        return customerRepository.GetAllAsync();
    }
}
