namespace zenvy.application.DTOs.Auth;
public class LoginResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string UserId { get; set; } = Guid.NewGuid().ToString();

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "Admin";

    public string AccessToken => Token;

    public string RefreshToken { get; set; } = string.Empty;

    public int ExpiresInSeconds { get; set; }
}