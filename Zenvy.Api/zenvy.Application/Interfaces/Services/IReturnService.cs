using zenvy.application.DTOs.Returns;

namespace zenvy.application.Interfaces.Services;

public interface IReturnService
{
    Task<long> CreateReturnAsync(ReturnRequest request);
    Task<IEnumerable<ReturnResponse>> GetReturnsAsync();
    Task<ReturnResponse?> GetReturnByIdAsync(long returnId);
    Task<bool> UpdateReturnStatusAsync(long returnId, ReturnStatusRequest request);
}
