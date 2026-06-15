using zenvy.application.DTOs.Payments;

namespace zenvy.application.Interfaces.Services;

public interface IPaymentService
{
    Task<long> CreatePaymentAsync(PaymentRequest request);
    Task<IEnumerable<PaymentResponse>> GetPaymentsAsync();
}
