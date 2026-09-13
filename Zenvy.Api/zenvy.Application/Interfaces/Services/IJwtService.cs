using zenvy.domain.Entities;

namespace zenvy.application.Interfaces.Services;
public interface IJwtService
{
    string GenerateToken(User user);

    int AccessTokenLifetimeSeconds { get; }
}