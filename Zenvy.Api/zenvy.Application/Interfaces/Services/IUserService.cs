using zenvy.application.DTOs.Users;

namespace zenvy.application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto> RegisterAsync(RegisterUserDto dto);

    Task<UserDto> RegisterManagerRequestAsync(ManagerSignupDto dto);

    Task<bool> SetActiveAsync(Guid id, bool active);

    Task<UserDto?> UpdateAsync(Guid id,
                              UpdateUserDto dto);

    Task DeleteAsync(Guid id);

    Task<UserDto?> GetByIdAsync(Guid id);

    Task<List<UserDto>> GetUsersAsync();
}
