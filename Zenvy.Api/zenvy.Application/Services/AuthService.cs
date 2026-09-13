using BCrypt.Net;
using zenvy.application.DTOs.Auth;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

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
        var refreshToken = CreateRefreshToken(user.UserId);
        await _userRepository.AddRefreshTokenAsync(refreshToken.Record);

        return new LoginResponse
        {
            Success = true,
            Message = "Login Successful",
            Token = token,
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
            ,RefreshToken = refreshToken.RawToken
            ,ExpiresInSeconds = _jwtService.AccessTokenLifetimeSeconds
        };
    }

    public async Task<LoginResponse?> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;
        var hash = HashRefreshToken(refreshToken);
        var stored = await _userRepository.GetRefreshTokenAsync(hash);
        if (stored is null || stored.RevokedAt is not null || stored.ExpiresAt <= DateTime.UtcNow) return null;
        var user = await _userRepository.GetByIdAsync(Guid.Parse(stored.UserId));
        if (user is null || !user.IsActive) return null;

        await _userRepository.RevokeRefreshTokenAsync(hash, DateTime.UtcNow);
        var replacement = CreateRefreshToken(user.UserId);
        await _userRepository.AddRefreshTokenAsync(replacement.Record);
        return new LoginResponse
        {
            Success = true, Message = "Token refreshed", Token = _jwtService.GenerateToken(user),
            RefreshToken = replacement.RawToken, ExpiresInSeconds = _jwtService.AccessTokenLifetimeSeconds,
            UserId = user.UserId, FullName = user.FullName, Email = user.Email, Role = user.Role
        };
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return false;
        var hash = HashRefreshToken(refreshToken);
        var stored = await _userRepository.GetRefreshTokenAsync(hash);
        if (stored is null || stored.RevokedAt is not null) return false;
        await _userRepository.RevokeRefreshTokenAsync(hash, DateTime.UtcNow);
        return true;
    }

    private static (string RawToken, zenvy.domain.Entities.RefreshToken Record) CreateRefreshToken(string userId)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (raw, new zenvy.domain.Entities.RefreshToken { TokenHash = HashRefreshToken(raw), UserId = userId, ExpiresAt = DateTime.UtcNow.AddDays(30) });
    }

    private static string HashRefreshToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
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