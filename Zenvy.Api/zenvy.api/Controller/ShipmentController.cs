using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Shipments;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize]
[Route("api/shipments")]
[ApiController]
public class ShipmentController(IShipmentService shipmentService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateShipment([FromBody] ShipmentRequest request)
    {
        var shipmentId = await shipmentService.CreateShipmentAsync(request);
        return Ok(new { ShipmentId = shipmentId });
    }

    [HttpGet]
    public async Task<IActionResult> GetShipments()
    {
        var response = await shipmentService.GetShipmentsAsync();
        return Ok(response);
    }
}
