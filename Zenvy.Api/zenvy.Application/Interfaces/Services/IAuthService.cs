using zenvy.application.DTOs.Auth;
using zenvy.application.Interfaces.Services;
namespace zenvy.Application.Auth
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<UserProfileResponse> GetProfileAsync(string userId);
    }
}