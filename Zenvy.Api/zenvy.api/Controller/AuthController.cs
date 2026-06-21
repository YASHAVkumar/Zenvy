using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Auth;
using zenvy.Application.Auth;
using zenvy.Domain.DTOs;

namespace zenvy.api.Controller;

[Route("api/v{version:apiVersion}/auth")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authService.LoginAsync(request);
        if (response == null)
        {
            return Unauthorized(new { Message = "Invalid email or password" });
        }
        return Ok(response);
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
