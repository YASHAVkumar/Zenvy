using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Finance;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize, ApiController, Route("api/v{version:apiVersion}/employee-commissions")]
public class EmployeeCommissionController(IEmployeeCommissionService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(EmployeeCommissionRequest request)
    {
        if (request.CommissionPercent is <= 0 or > 100) return BadRequest("CommissionPercent must be between 0 and 100.");
        return Ok(new { CommissionId = await service.CreateAsync(request) });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? userId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        if (fromDate > toDate) return BadRequest("fromDate cannot be after toDate.");
        return Ok(await service.GetAllAsync(userId, fromDate, toDate));
    }
}
