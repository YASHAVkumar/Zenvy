using BCrypt.Net;
using zenvy.application.DTOs.Auth;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;

namespace zenvy.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }
    public async Task<LoginResponse> LoginAsync(
       LoginRequest request)
    {
        var user =
            await _userRepository
                .GetByEmailAsync(request.Email);

        if (user == null)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid Email"
            };
        }

        bool passwordValid =
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

        if (!passwordValid)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid Password"
            };
        }

        if (!user.IsActive)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Your manager account is waiting for admin approval."
            };
        }

        string token = _jwtService.GenerateToken(user);

        return new LoginResponse
        {
            Success = true,
            Message = "Login Successful",
            Token = token,
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
    public async Task<UserProfileResponse> GetProfileAsync(string userId)
    {
        var user =
            await _userRepository.GetByIdAsync(
                Guid.Parse(userId));

        return new UserProfileResponse
        {
            UserId = user!.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
    public async Task<bool> ChangePasswordAsync(string userId, ChangePassRequest request)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId)) ?? throw new Exception("User not found");
        bool passwordValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash);
        if (!passwordValid)
        {
            throw new Exception("Current password is incorrect");
        }

        string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        return await _userRepository.ChaangePasswordAsync(Guid.Parse(userId), newHashedPassword);
    }

}