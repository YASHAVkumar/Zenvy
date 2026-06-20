using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize, ApiController, Route("api/profit")]
public class ProfitController(IProfitService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        if (fromDate == default || toDate == default) return BadRequest("fromDate and toDate are required.");
        if (fromDate > toDate) return BadRequest("fromDate cannot be after toDate.");
        return Ok(await service.GetSummaryAsync(fromDate, toDate));
    }
}
