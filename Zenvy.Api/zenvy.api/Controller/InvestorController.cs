using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Finance;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize, ApiController, Route("api/v{version:apiVersion}/investors")]
public class InvestorController(IInvestorService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(InvestorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required.");
        if (request.OwnershipPercent is <= 0 or > 100) return BadRequest("OwnershipPercent must be between 0 and 100.");
        
        if (request.LossPercent is <0 or > 100) return BadRequest("LossPercent must be between 0 and 100.");
        
        return Ok(new { InvestorId = await service.CreateAsync(request) });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

    [HttpPost("profit-distributions")]
    public async Task<IActionResult> DistributeProfit(ProfitDistributionRequest request)
    {
        if (request.Month is < 1 or > 12 || request.Year < 2000) return BadRequest("A valid month and year are required.");
        return Ok(new { DistributionCount = await service.DistributeProfitAsync(request) });
    }

    [HttpGet("profit-distributions")]
    public async Task<IActionResult> GetDistributions([FromQuery] short? year, [FromQuery] byte? month, [FromQuery] int? investorId) =>
        Ok(await service.GetDistributionsAsync(year, month, investorId));
}
