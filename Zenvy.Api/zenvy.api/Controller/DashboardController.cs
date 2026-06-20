using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize, ApiController, Route("api/v{version:apiVersion}/dashboard")]
public class DashboardController(IDashboardService service) : ControllerBase
{
    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] int lowStockThreshold = 10)
    {
        if (fromDate == default || toDate == default) return BadRequest("fromDate and toDate are required.");
        if (fromDate > toDate) return BadRequest("fromDate cannot be after toDate.");
        if (lowStockThreshold < 0) return BadRequest("lowStockThreshold cannot be negative.");
        return Ok(await service.GetSummaryAsync(fromDate, toDate, lowStockThreshold));
    }
}
