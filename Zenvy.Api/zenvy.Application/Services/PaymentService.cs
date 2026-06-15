using zenvy.application.DTOs.Payments;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;

namespace zenvy.Application.Services;

public class PaymentService(IPaymentRepository paymentRepository) : IPaymentService
{
    public Task<long> CreatePaymentAsync(PaymentRequest request)
    {
        return paymentRepository.CreateAsync(request);
    }

    public Task<IEnumerable<PaymentResponse>> GetPaymentsAsync()
    {
        return paymentRepository.GetAllAsync();
    }
}
