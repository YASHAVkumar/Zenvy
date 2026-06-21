using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Returns;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize]
[Route("api/v{version:apiVersion}/returns")]
[ApiController]
public class ReturnController(IReturnService returnService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateReturn([FromBody] ReturnRequest request)
    {
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
