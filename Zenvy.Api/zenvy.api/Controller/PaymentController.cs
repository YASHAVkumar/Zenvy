using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Payments;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Authorize]
[Route("api/payments")]
[ApiController]
public class PaymentController(IPaymentService paymentService) : ControllerBase
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
}
