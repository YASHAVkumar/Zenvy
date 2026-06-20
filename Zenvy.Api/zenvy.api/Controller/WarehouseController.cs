using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Warehouses;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize]
[Route("api/v{version:apiVersion}/warehouses")]
[ApiController]
public class WarehouseController(IWarehouseService warehouseService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateWarehouse([FromBody] WarehouseRequest request)
    {
        var warehouseId = await warehouseService.CreateWarehouseAsync(request);
        return Ok(new { WarehouseId = warehouseId });
    }

    [HttpGet]
    public async Task<IActionResult> GetWarehouses()
    {
        var response = await warehouseService.GetWarehousesAsync();
        return Ok(response);
    }

    [HttpGet("{warehouseId:int}")]
    public async Task<IActionResult> GetWarehouseById(int warehouseId)
    {
        var response = await warehouseService.GetWarehouseByIdAsync(warehouseId);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpPut("{warehouseId:int}")]
    public async Task<IActionResult> UpdateWarehouse(int warehouseId, [FromBody] WarehouseRequest request)
    {
        var success = await warehouseService.UpdateWarehouseAsync(warehouseId, request);
        return Ok(new { Success = success });
    }

    [HttpDelete("{warehouseId:int}")]
    public async Task<IActionResult> DeleteWarehouse(int warehouseId)
    {
        var success = await warehouseService.DeleteWarehouseAsync(warehouseId);
        return Ok(new { Success = success });
    }

    [HttpGet("dropdown")]
    public async Task<IActionResult> GetWarehouseDropdown()
    {
        var response = await warehouseService.GetWarehouseDropdownAsync();
        return Ok(response);
    }
}
