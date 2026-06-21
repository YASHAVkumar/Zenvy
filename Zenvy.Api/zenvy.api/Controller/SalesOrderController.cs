using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.SalesOrders;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize]
[Route("api/v{version:apiVersion}/sales-orders")]
[ApiController]
public class SalesOrderController(ISalesOrderService salesOrderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateSalesOrder([FromBody] SalesOrderRequest request)
    {
        var orderId = await salesOrderService.CreateSalesOrderAsync(request);
        return Ok(new { OrderId = orderId });
    }

    [HttpGet]
    public async Task<IActionResult> GetSalesOrders()
    {
        var response = await salesOrderService.GetSalesOrdersAsync();
        return Ok(response);
    }

    [HttpGet("{orderId:long}")]
    public async Task<IActionResult> GetSalesOrderById(long orderId)
    {
        var response = await salesOrderService.GetSalesOrderByIdAsync(orderId);
        if (response == null) return NotFound();
        return Ok(response);
    }
}
