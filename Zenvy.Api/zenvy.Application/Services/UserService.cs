using zenvy.application.DTOs.Users;
using zenvy.application.Interfaces;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;
using zenvy.domain.Entities;

namespace zenvy.application.Service;

public class UserService(IUserRepository repository, IUnitOfWork unitOfWork) : IUserService
{
    public async Task<UserDto> RegisterAsync(
        RegisterUserDto dto)
    {
        User user = new()
        {
            UserId = Guid.NewGuid().ToString(),
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = dto.Role.RoleId,
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
            Role = dto.Role.Name
        };
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
        })];
    }

    public async Task DeleteAsync(Guid id)
    {
        await repository.DeleteAsync(id);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await repository.GetByIdAsync(id);

        return new UserDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<UserDto> UpdateAsync(
        Guid id,
        UpdateUserDto dto)
    {
        var user =
            await repository.GetByIdAsync(id);

        user.FullName = dto.FullName;
        user.Phone = dto.Phone;

        await repository.UpdateAsync(user);

        await unitOfWork.SaveChangesAsync();

        return new UserDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
}
