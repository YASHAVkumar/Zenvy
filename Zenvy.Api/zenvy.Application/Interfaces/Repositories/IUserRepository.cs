using System;
using System.Collections.Generic;
using System.Text;
using zenvy.domain.Entities;

namespace zenvy.application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);

        Task<User?> GetByEmailAsync(string email);

        Task<List<User>> GetAllAsync();

        Task AddAsync(User user);

        Task UpdateAsync(User user);

        Task DeleteAsync(Guid id);
        Task<bool> ChaangePasswordAsync(Guid userId, string newPasswordHash);
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);
        Task RevokeRefreshTokenAsync(string tokenHash, DateTime revokedAt);
    }
}
