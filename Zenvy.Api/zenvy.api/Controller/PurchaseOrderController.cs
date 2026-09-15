using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.PurchaseOrders;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize(Roles = "Admin,Manager,InventoryManager,Accountant,TeamLead")]
[Route("api/v{version:apiVersion}/purchase-orders")]
[ApiController]
public class PurchaseOrderController(IPurchaseOrderService purchaseOrderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePurchaseOrder([FromBody] PurchaseOrderRequest request)
    {
        if (request is null) return BadRequest("Purchase order is required.");
        if (request.SupplierId <= 0) return BadRequest("SupplierId must be greater than zero.");
        if (request.WarehouseId <= 0) return BadRequest("WarehouseId must be greater than zero.");
        if (string.IsNullOrWhiteSpace(request.PONumber)) return BadRequest("PONumber is required.");
        if (request.Lines == null || request.Lines.Count == 0) return BadRequest("At least one purchase line is required.");
        if (request.Lines.Any(line => line.VariantId <= 0 || line.Qty <= 0 || line.UnitCost < 0 || line.TaxAmount < 0))
            return BadRequest("Each purchase line must have a valid variant, positive quantity, non-negative unit cost and tax.");
        if (!Guid.TryParse(request.CreatedBy, out _)) return BadRequest("CreatedBy must be a valid GUID.");

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
