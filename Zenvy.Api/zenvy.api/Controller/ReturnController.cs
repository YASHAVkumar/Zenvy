using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Returns;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize(Roles = "Admin,Manager,Accountant,SalesPerson,TeamLead")]
[Route("api/v{version:apiVersion}/returns")]
[ApiController]
public class ReturnController(IReturnService returnService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateReturn([FromBody] ReturnRequest request)
    {
        if (request is null) return BadRequest("Return request is required.");
        if (request.OrderId <= 0) return BadRequest("OrderId must be greater than zero.");
        if (request.Lines == null || request.Lines.Count == 0) return BadRequest("At least one return line is required.");
        if (request.Lines.Any(line => line.OrderLineId <= 0 || line.Qty <= 0 || line.RefundAmount < 0))
            return BadRequest("Each return line must include a valid order line, positive quantity, and non-negative refund amount.");
        if (request.CreatedBy is not null && !Guid.TryParse(request.CreatedBy, out _)) return BadRequest("CreatedBy must be a valid GUID when provided.");

        var returnId = await returnService.CreateReturnAsync(request);
        return Ok(new { ReturnId = returnId });
    }

    [HttpGet]
    public async Task<IActionResult> GetReturns()
    {
        var response = await returnService.GetReturnsAsync();
        return Ok(response);
    }

    [HttpGet("{returnId:long}")]
    public async Task<IActionResult> GetReturnById(long returnId)
    {
        var response = await returnService.GetReturnByIdAsync(returnId);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpPut("{returnId:long}/status")]
    public async Task<IActionResult> UpdateReturnStatus(long returnId, [FromBody] ReturnStatusRequest request)
    {
        var success = await returnService.UpdateReturnStatusAsync(returnId, request);
        return Ok(new { Success = success });
    }
}
