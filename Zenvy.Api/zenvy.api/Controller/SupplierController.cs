using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Suppliers;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize]
[Route("api/suppliers")]
[ApiController]
public class SupplierController(ISupplierService supplierService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] SupplierRequest request)
    {
        var supplierId = await supplierService.CreateSupplierAsync(request);
        return Ok(new { SupplierId = supplierId });
    }

    [HttpGet]
    public async Task<IActionResult> GetSuppliers()
    {
        var response = await supplierService.GetSuppliersAsync();
        return Ok(response);
    }
}
