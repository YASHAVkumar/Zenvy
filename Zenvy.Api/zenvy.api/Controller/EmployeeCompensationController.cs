using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize(Roles = "Admin,Manager,Accountant")]
[ApiController]
[Route("api/v{version:apiVersion}/employee-reports")]
public class EmployeeCompensationController(IEmployeeCompensationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetReport([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int? employeeId)
    {
        var end = (toDate ?? DateTime.UtcNow).Date;
        var start = (fromDate ?? new DateTime(end.Year, end.Month, 1)).Date;
        if (start > end) return BadRequest("fromDate must be before or equal to toDate.");

        return Ok(await service.GetReportAsync(start, end, employeeId));
    }
}
