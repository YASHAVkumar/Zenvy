using zenvy.application.DTOs.Returns;

namespace zenvy.application.Interfaces.Repositories;

public interface IReturnRepository
{
    Task<long> CreateAsync(ReturnRequest request);
    Task<IEnumerable<ReturnResponse>> GetAllAsync();
    Task<ReturnResponse?> GetByIdAsync(long returnId);
    Task<bool> UpdateStatusAsync(long returnId, ReturnStatusRequest request);
}
