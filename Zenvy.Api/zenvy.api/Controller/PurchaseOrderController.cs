using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.PurchaseOrders;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize]
[Route("api/v{version:apiVersion}/purchase-orders")]
[ApiController]
public class PurchaseOrderController(IPurchaseOrderService purchaseOrderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePurchaseOrder([FromBody] PurchaseOrderRequest request)
    {
        var poId = await purchaseOrderService.CreatePurchaseOrderAsync(request);
        return Ok(new { POId = poId });
    }

    [HttpGet]
    public async Task<IActionResult> GetPurchaseOrders()
    {
        var response = await purchaseOrderService.GetPurchaseOrdersAsync();
        return Ok(response);
    }

    [HttpGet("{poId:long}")]
    public async Task<IActionResult> GetPurchaseOrderById(long poId)
    {
        var response = await purchaseOrderService.GetPurchaseOrderByIdAsync(poId);
        if (response == null) return NotFound();
        return Ok(response);
    }
}
