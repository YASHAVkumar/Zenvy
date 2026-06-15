using zenvy.application.DTOs.Payments;

namespace zenvy.application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<long> CreateAsync(PaymentRequest request);
    Task<IEnumerable<PaymentResponse>> GetAllAsync();
}
