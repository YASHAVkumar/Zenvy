using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Customers;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize]
[Route("api/v{version:apiVersion}/customers")]
[ApiController]
public class CustomerController(ICustomerService customerService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CustomerRequest request)
    {
        var customerId = await customerService.CreateCustomerAsync(request);
        return Ok(new { CustomerId = customerId });
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var response = await customerService.GetCustomersAsync();
        return Ok(response);
    }
}
