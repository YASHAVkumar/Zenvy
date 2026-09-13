using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Auth;
using zenvy.Application.Auth;
using zenvy.Domain.DTOs;
using zenvy.application.DTOs.Users;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;

[Route("api/v{version:apiVersion}/auth")]
[ApiController]
public class AuthController(IAuthService authService, IUserService userService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authService.LoginAsync(request);
        if (response is null || !response.Success)
        {
            return Unauthorized(response ?? new LoginResponse
            {
                Success = false,
                Message = "Invalid email or password"
            });
        }
        return Ok(response);
    }

    [HttpPost("signup/manager")]
    public async Task<IActionResult> SignupManager([FromBody] ManagerSignupDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email) || request.Password.Length < 8)
            return BadRequest("Full name, email, and a password of at least 8 characters are required.");
        var result = await userService.RegisterManagerRequestAsync(request);
        return Ok(new { message = "Manager request submitted for admin approval.", user = result });
    }
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var userId =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        var result =
            await authService.GetProfileAsync(userId!);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePassRequest request)
    {
        var userId =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;
        
        bool success = await authService.ChangePasswordAsync(userId!, request);

        //return Ok(new { Message = success ? "Password changed successfully" : "Failed to change password" });
        return Ok(new BaseResponse
        {
            Success = success,
            Message = success ? "Password changed successfully" : "Failed to change password"
        });
    }
}
