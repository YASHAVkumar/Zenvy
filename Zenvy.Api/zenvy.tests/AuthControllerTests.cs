using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using zenvy.api.Controller;
using zenvy.Application.Auth;
using zenvy.application.DTOs.Auth;
using zenvy.application.DTOs.Users;
using zenvy.application.Interfaces.Services;

namespace zenvy.tests;

public sealed class AuthControllerTests
{
    private readonly Mock<IAuthService> _auth = new();
    private readonly Mock<IUserService> _users = new();
    private AuthController CreateSubject() => new(_auth.Object, _users.Object);

    [Fact]
    public async Task Login_returns_401_when_service_rejects_credentials()
    {
        _auth.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(new LoginResponse { Success = false, Message = "Invalid Password" });

        var result = await CreateSubject().Login(new LoginRequest { Email = "user@example.com", Password = "bad" });

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(unauthorized.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Login_returns_200_with_credentials_when_service_accepts_request()
    {
        var response = new LoginResponse { Success = true, Token = "access-token", RefreshToken = "refresh-token" };
        _auth.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(response);

        var result = await CreateSubject().Login(new LoginRequest { Email = "user@example.com", Password = "good" });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task Refresh_returns_401_for_an_invalid_refresh_token()
    {
        _auth.Setup(x => x.RefreshAsync("expired")).ReturnsAsync((LoginResponse?)null);

        var result = await CreateSubject().Refresh(new RefreshTokenRequest { RefreshToken = "expired" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task SignupManager_rejects_incomplete_or_short_password_requests_before_service_call()
    {
        var result = await CreateSubject().SignupManager(new ManagerSignupDto
        {
            FullName = "", Email = "user@example.com", Password = "short"
        });

        Assert.IsType<BadRequestObjectResult>(result);
        _users.Verify(x => x.RegisterManagerRequestAsync(It.IsAny<ManagerSignupDto>()), Times.Never);
    }
}
