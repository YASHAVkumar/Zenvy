using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Inventory;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize]
[Route("api/v{version:apiVersion}/inventory")]
[ApiController]
public class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    // [HttpPost]
    // public async Task<IActionResult> CreateInventory([FromBody] InventoryRequest request)
    // {
    //     var inventoryId = await inventoryService.CreateInventoryAsync(request);
    //     return Ok(new { InventoryId = inventoryId });
    // }

    [HttpGet]
    public async Task<IActionResult> GetInventory()
    {
        var response = await inventoryService.GetInventoryAsync();
        return Ok(response);
    }

    [HttpGet("{inventoryId:int}")]
    public async Task<IActionResult> GetInventoryById(int inventoryId)
    {
        var response = await inventoryService.GetInventoryByIdAsync(inventoryId);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpPut("{inventoryId:int}")]
    public async Task<IActionResult> UpdateInventory(int inventoryId, [FromBody] InventoryRequest request)
    {
        var success = await inventoryService.UpdateInventoryAsync(inventoryId, request);
        return Ok(new { Success = success });
    }

    [HttpPost("adjustment")]
    public async Task<IActionResult> AdjustInventory([FromBody] InventoryAdjustmentRequest request)
    {
        var success = await inventoryService.AdjustInventoryAsync(request);
        return Ok(new { Success = success });
    }

    [HttpPost("damage")]
    public async Task<IActionResult> DamageInventory([FromBody] InventoryDamageRequest request)
    {
        var success = await inventoryService.DamageInventoryAsync(request);
        return Ok(new { Success = success });
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> TransferInventory([FromBody] InventoryTransferRequest request)
    {
        var success = await inventoryService.TransferInventoryAsync(request);
        return Ok(new { Success = success });
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetInventoryTransactions()
    {
        var response = await inventoryService.GetInventoryTransactionsAsync();
        return Ok(response);
    }

    [HttpGet("transactions/{variantId:int}")]
    public async Task<IActionResult> GetInventoryTransactionsByVariant(int variantId)
    {
        var response = await inventoryService.GetInventoryTransactionsByVariantAsync(variantId);
        return Ok(response);
    }
}
