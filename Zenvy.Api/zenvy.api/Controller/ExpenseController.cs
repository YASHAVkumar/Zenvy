using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Finance;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize(Roles = "Admin,Manager,Accountant,TeamLead"), ApiController, Route("api/v{version:apiVersion}/expense-types")]
public class ExpenseTypeController(IExpenseService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(ExpenseTypeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required.");
        return Ok(new { ExpenseTypeId = await service.CreateTypeAsync(request) });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await service.GetTypesAsync());
}

[Authorize(Roles = "Admin,Manager,Accountant,TeamLead"), ApiController, Route("api/v{version:apiVersion}/expenses")]
public class ExpenseController(IExpenseService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ExpenseRequest request)
    {
        if (request.Amount <= 0) return BadRequest("Amount must be greater than zero.");
        if (request.POId <= 0) return BadRequest("POId must be greater than zero.");
        if (request.ExpenseTypeId <= 0) return BadRequest("ExpenseTypeId must be greater than zero.");
        if (!Guid.TryParse(request.CreatedBy, out _)) return BadRequest("CreatedBy must be a valid GUID.");
        return Ok(new { ExpenseId = await service.CreateExpenseAsync(request) });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int? expenseTypeId)
    {
        if (fromDate > toDate) return BadRequest("fromDate cannot be after toDate.");
        return Ok(await service.GetExpensesAsync(fromDate, toDate, expenseTypeId));
    }
}
