using BCrypt.Net;
using Moq;
using Xunit;
using zenvy.Application.Auth;
using zenvy.application.DTOs.Auth;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;
using zenvy.domain.Entities;

namespace zenvy.tests;

public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IJwtService> _jwt = new();

    private AuthService CreateSubject()
    {
        _jwt.SetupGet(x => x.AccessTokenLifetimeSeconds).Returns(900);
        return new AuthService(_users.Object, _jwt.Object);
    }

    [Fact]
    public async Task LoginAsync_rejects_unknown_email_without_issuing_a_token()
    {
        _users.Setup(x => x.GetByEmailAsync("missing@example.com")).ReturnsAsync((User?)null);

        var result = await CreateSubject().LoginAsync(new LoginRequest { Email = "missing@example.com", Password = "password" });

        Assert.False(result.Success);
        Assert.Equal("Invalid Email", result.Message);
        _jwt.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_rejects_wrong_password()
    {
        _users.Setup(x => x.GetByEmailAsync("user@example.com")).ReturnsAsync(ActiveUser());

        var result = await CreateSubject().LoginAsync(new LoginRequest { Email = "user@example.com", Password = "incorrect" });

        Assert.False(result.Success);
        Assert.Equal("Invalid Password", result.Message);
        _users.Verify(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_rejects_inactive_user()
    {
        var user = ActiveUser();
        user.IsActive = false;
        _users.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var result = await CreateSubject().LoginAsync(new LoginRequest { Email = user.Email, Password = "correct-password" });

        Assert.False(result.Success);
        Assert.Contains("waiting for admin approval", result.Message);
        _jwt.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_issues_access_and_hashed_refresh_tokens_for_active_user()
    {
        var user = ActiveUser();
        RefreshToken? savedToken = null;
        _users.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _users.Setup(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).Callback<RefreshToken>(token => savedToken = token).Returns(Task.CompletedTask);
        _jwt.Setup(x => x.GenerateToken(user)).Returns("access-token");

        var result = await CreateSubject().LoginAsync(new LoginRequest { Email = user.Email, Password = "correct-password" });

        Assert.True(result.Success);
        Assert.Equal("access-token", result.Token);
        Assert.Equal(900, result.ExpiresInSeconds);
        Assert.NotEmpty(result.RefreshToken);
        Assert.NotNull(savedToken);
        Assert.NotEqual(result.RefreshToken, savedToken!.TokenHash);
        Assert.Equal(user.UserId, savedToken.UserId);
        Assert.InRange(savedToken.ExpiresAt, DateTime.UtcNow.AddDays(29), DateTime.UtcNow.AddDays(31));
    }

    [Fact]
    public async Task RefreshAsync_returns_null_for_blank_or_missing_token()
    {
        Assert.Null(await CreateSubject().RefreshAsync(""));

        _users.Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);
        Assert.Null(await CreateSubject().RefreshAsync("unknown"));
        _users.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_rotates_valid_token_and_returns_new_credentials()
    {
        var user = ActiveUser();
        var token = new RefreshToken { UserId = user.UserId, TokenHash = "hash", ExpiresAt = DateTime.UtcNow.AddMinutes(5) };
        _users.Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync(token);
        _users.Setup(x => x.GetByIdAsync(Guid.Parse(user.UserId))).ReturnsAsync(user);
        _users.Setup(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _users.Setup(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _jwt.Setup(x => x.GenerateToken(user)).Returns("new-access-token");

        var result = await CreateSubject().RefreshAsync("valid-token");

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("new-access-token", result.Token);
        Assert.NotEmpty(result.RefreshToken);
        _users.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
        _users.Verify(x => x.AddRefreshTokenAsync(It.Is<RefreshToken>(item => item.TokenHash != "hash")), Times.Once);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_returns_false_for_already_revoked_token()
    {
        _users.Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync(new RefreshToken { RevokedAt = DateTime.UtcNow });

        var result = await CreateSubject().RevokeRefreshTokenAsync("already-revoked");

        Assert.False(result);
        _users.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    private static User ActiveUser() => new()
    {
        UserId = "87c9165d-7f3f-4b1e-999d-0c9d8a182855",
        Email = "user@example.com",
        FullName = "Test User",
        Role = "Admin",
        IsActive = true,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password")
    };
}
