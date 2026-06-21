using zenvy.application.DTOs.Returns;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;

namespace zenvy.Application.Services;

public class ReturnService(IReturnRepository returnRepository) : IReturnService
{
    public Task<long> CreateReturnAsync(ReturnRequest request)
    {
        return returnRepository.CreateAsync(request);
    }

    public Task<IEnumerable<ReturnResponse>> GetReturnsAsync()
    {
        return returnRepository.GetAllAsync();
    }

    public Task<ReturnResponse?> GetReturnByIdAsync(long returnId)
    {
        return returnRepository.GetByIdAsync(returnId);
    }

    public Task<bool> UpdateReturnStatusAsync(long returnId, ReturnStatusRequest request)
    {
        return returnRepository.UpdateStatusAsync(returnId, request);
    }
}
