using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using zenvy.application.DTOs.Payments;
using zenvy.application.Interfaces.Services;
using zenvy.api.Hubs;

namespace zenvy.api.Controller;

[Authorize]
[Route("api/v{version:apiVersion}/payments")]
[ApiController]
public class PaymentController(IPaymentService paymentService, IHubContext<NotificationHub> notificationHub) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
    {
        var paymentId = await paymentService.CreatePaymentAsync(request);
        return Ok(new { PaymentId = paymentId });
    }

    [HttpGet]
    public async Task<IActionResult> GetPayments()
    {
        var response = await paymentService.GetPaymentsAsync();
        return Ok(response);
    }

    [HttpPut("{paymentId:long}/status")]
    [Authorize(Roles = "Admin,Manager,Accountant")]
    public async Task<IActionResult> UpdatePaymentStatus(long paymentId, [FromBody] PaymentStatusUpdateRequest request)
    {
        if (paymentId <= 0) return BadRequest("PaymentId must be greater than zero.");
        if (string.IsNullOrWhiteSpace(request.Status)
            || !Enum.TryParse<zenvy.Domain.Enums.PaymentStatus>(request.Status, true, out var paymentStatus)
            || !Enum.IsDefined(paymentStatus))
        {
            return BadRequest("Status must be a valid payment status.");
        }

        var status = zenvy.Domain.Enums.EnumMappings.GetPaymentStatusValue(paymentStatus);
        var transactionRef = string.IsNullOrWhiteSpace(request.TransactionRef) ? null : request.TransactionRef.Trim();
        if (!await paymentService.UpdateStatusAsync(paymentId, status, transactionRef))
        {
            return NotFound();
        }

        await notificationHub.Clients.All.SendAsync("PaymentStatusChanged", new
        {
            PaymentId = paymentId,
            Status = status,
            UpdatedBy = User.Identity?.Name ?? "A team member"
        });

        return NoContent();
    }
}
