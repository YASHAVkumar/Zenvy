using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize(Roles = "Admin,Manager,Accountant,TeamLead")]
[Route("api/v{version:apiVersion}/dashboards")]
[ApiController]
public class DashboardController(IDashboardService service) : ControllerBase
{
    /// <summary>
    /// Get Executive/Admin Dashboard - Overall business health
    /// </summary>
    [HttpGet("executive")]
    public async Task<IActionResult> GetExecutiveDashboard()
    {
        var response = await service.GetExecutiveDashboardAsync();
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// Get Finance Dashboard - Money management and fund flow
    /// </summary>
    [HttpGet("finance")]
    public async Task<IActionResult> GetFinanceDashboard()
    {
        var response = await service.GetFinanceDashboardAsync();
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// Get Operations Dashboard - Daily business operations
    /// </summary>
    [HttpGet("operations")]
    public async Task<IActionResult> GetOperationsDashboard()
    {
        var response = await service.GetOperationsDashboardAsync();
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// Get Marketplace Dashboard - Meesho, Myntra, Amazon tracking
    /// </summary>
    [HttpGet("marketplace")]
    public async Task<IActionResult> GetMarketplaceDashboard()
    {
        var response = await service.GetMarketplaceDashboardAsync();
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// Get Investor Dashboard - Investment and profit tracking
    /// </summary>
    [HttpGet("investor/{investorId:int}")]
    public async Task<IActionResult> GetInvestorDashboard(int investorId)
    {
        var response = await service.GetInvestorDashboardAsync(investorId);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// Get Employee Dashboard - Task and performance tracking
    /// </summary>
    [HttpGet("employee")]
    public async Task<IActionResult> GetEmployeeDashboard([FromQuery] int? employeeId = null)
    {
        var response = await service.GetEmployeeDashboardAsync(employeeId);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }
}
