using zenvy.application.DTOs.Users;
using zenvy.application.Interfaces;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;
using zenvy.domain.Entities;
using zenvy.Domain.Enums;

namespace zenvy.application.Service;

public class UserService(IUserRepository repository, IUnitOfWork unitOfWork) : IUserService
{
    public async Task<UserDto> RegisterAsync(
        RegisterUserDto dto)
    {
        var role = ValidateRole(dto.Role);
        User user = new()
        {
            UserId = Guid.NewGuid().ToString(),
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = (int)role,
            Role = role.ToString(),
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        await repository.AddAsync(user);

        await unitOfWork.SaveChangesAsync();

        return new UserDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = role.ToString()
            ,IsActive = user.IsActive
        };
    }

    public Task<UserDto> RegisterManagerRequestAsync(ManagerSignupDto dto) => RegisterPendingAsync(dto);

    private async Task<UserDto> RegisterPendingAsync(ManagerSignupDto dto)
    {
        User user = new()
        {
            UserId = Guid.NewGuid().ToString(), FullName = dto.FullName, Email = dto.Email, Phone = dto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), RoleId = 2, Role = UserRoles.Manager.ToString(),
            IsActive = false, CreatedAt = DateTime.Now
        };
        await repository.AddAsync(user); await unitOfWork.SaveChangesAsync();
        return new UserDto { UserId = user.UserId, FullName = user.FullName, Email = user.Email, Role = user.Role, IsActive = false };
    }

    public async Task<bool> SetActiveAsync(Guid id, bool active)
    {
        var user = await repository.GetByIdAsync(id);
        if (user is null) return false;
        user.IsActive = active;
        await repository.UpdateAsync(user); await unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<List<UserDto>> GetUsersAsync()
    {
        var users = await repository.GetAllAsync();

        return [.. users.Select(x => new UserDto
        {
            UserId = x.UserId,
            FullName = x.FullName,
            Email = x.Email,
            Role = x.Role
            ,IsActive = x.IsActive
        })];
    }

    public async Task DeleteAsync(Guid id)
    {
        await repository.DeleteAsync(id);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await repository.GetByIdAsync(id);

        if (user is null) return null;

        return new UserDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
            ,IsActive = user.IsActive
        };
    }

    public async Task<UserDto?> UpdateAsync(
        Guid id,
        UpdateUserDto dto)
    {
        var user =
            await repository.GetByIdAsync(id);

        if (user is null) return null;

        user.FullName = dto.FullName;
        user.Phone = dto.Phone;
        var role = ValidateRole(dto.Role);
        user.RoleId = (int)role;
        user.Role = role.ToString();

        await repository.UpdateAsync(user);

        await unitOfWork.SaveChangesAsync();

        return new UserDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
            ,IsActive = user.IsActive
        };
    }

    private static UserRoles ValidateRole(Role role)
    {
        if (role is null || !Enum.TryParse<UserRoles>(role.Name?.Trim(), true, out var parsedRole) || (int)parsedRole != role.RoleId)
            throw new ArgumentException("RoleId and role name must match a supported role.");
        return parsedRole;
    }
}
