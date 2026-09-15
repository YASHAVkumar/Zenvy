using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.SalesOrders;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize(Roles = "Admin,Manager,SalesPerson,TeamLead")]
[Route("api/v{version:apiVersion}/sales-orders")]
[ApiController]
public class SalesOrderController(ISalesOrderService salesOrderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateSalesOrder([FromBody] SalesOrderRequest request)
    {
        if (request is null) return BadRequest("Sales order is required.");
        if (request.ChannelId <= 0) return BadRequest("ChannelId must be greater than zero.");
        if (request.Lines == null || request.Lines.Count == 0) return BadRequest("At least one sales line is required.");
        if (request.Lines.Any(line => line.VariantId <= 0 || line.Qty <= 0 || line.UnitPrice < 0 || line.Discount < 0 || line.Tax < 0))
            return BadRequest("Each sales line must have a valid variant, positive quantity, and non-negative pricing values.");
        if (!Guid.TryParse(request.CreatedBy, out _)) return BadRequest("CreatedBy must be a valid GUID.");

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
