using System;
using System.Collections.Generic;
using System.Text;
using zenvy.application.Interfaces.Repositories;
using zenvy.domain.Entities;

namespace zenvy.infrastructure.Persistence.SqlServer.EF.Repository
{
    internal class EfUserRepositories : IUserRepository
    {
        public Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            throw new NotImplementedException();
        }

        public Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash)
        {
            throw new NotImplementedException();
        }

        public Task RevokeRefreshTokenAsync(string tokenHash, DateTime revokedAt)
        {
            throw new NotImplementedException();
        }

        Task IUserRepository.AddAsync(User user)
        {
            throw new NotImplementedException();
        }

        Task<bool> IUserRepository.ChaangePasswordAsync(Guid userId, string newPasswordHash)
        {
            throw new NotImplementedException();
        }

        Task IUserRepository.DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        Task<List<User>> IUserRepository.GetAllAsync()
        {
            throw new NotImplementedException();
        }

        Task<User?> IUserRepository.GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        Task<User?> IUserRepository.GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        Task IUserRepository.UpdateAsync(User user)
        {
            throw new NotImplementedException();
        }
    }
}
