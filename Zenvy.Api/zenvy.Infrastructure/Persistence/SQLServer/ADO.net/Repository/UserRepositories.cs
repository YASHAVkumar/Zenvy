
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using zenvy.application.Interfaces.Repositories;
using zenvy.domain.Entities;
using zenvy.shared;

namespace zenvy.infrastructure.Persistence.SqlServer.ADO.net.Repository
{
    public class UserRepositories(IConfiguration configuration) : DataAccessSetup,IUserRepository
    {
        private readonly string sqlConnectionString = configuration?.GetConnectionString("DefaultConnection")??"";

        public Task AddAsync(User user)
        {
            try
            {
                using var connection = new SqlConnection(sqlConnectionString);
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Users (UserId, FullName, Email, PasswordHash, Phone, RoleId, IsActive, CreatedAt,Role) VALUES (@UserId, @FullName, @Email, @PasswordHash, @Phone, @RoleId, @IsActive, @CreatedAt, @Role)";
                command.Parameters.AddWithValue("@UserId", user.UserId);
                command.Parameters.AddWithValue("@FullName", user.FullName);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@Phone", user.Phone as object ?? DBNull.Value);
                command.Parameters.AddWithValue("@RoleId", user.RoleId);
                command.Parameters.AddWithValue("@IsActive", user.IsActive);
                command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                command.Parameters.AddWithValue("@Role", user.Role);
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            //throw new NotImplementedException();
            try
            {
                using var connection = new SqlConnection(sqlConnectionString);
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Users WHERE UserId = @UserId";
                command.Parameters.AddWithValue("@UserId", id.ToString());

                connection.Open();
                if (command.ExecuteNonQuery() != 1)
                    throw new KeyNotFoundException("The user was not found.");
            }
            catch
            {
                throw;
            }
            return Task.CompletedTask;
        }

        public Task<List<User>> GetAllAsync()
        {
            try
            {
                using var connection = new SqlConnection(sqlConnectionString);
                var users = new List<User>();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Users";
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new User
                        {
                            UserId = reader["UserId"]?.ToString()??"",
                            RoleId = Convert.ToInt32(reader["RoleId"]),
                            FullName = reader["FullName"]?.ToString()??"",
                            Email = reader["Email"]?.ToString()??"",
                            PasswordHash = reader["PasswordHash"]?.ToString()??"",
                            Phone = reader["Phone"]?.ToString()??"",
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            Role = reader["Role"]?.ToString()??""
                        };
                        users.Add(user);
                    }
                }
                return Task.FromResult(users);
            }
            catch
            {
                throw;
            }
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            //throw new NotImplementedException();
            try
            {
                using var connection = new SqlConnection(sqlConnectionString);
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Users WHERE Email = @Email";
                command.Parameters.AddWithValue("@Email", email);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var user = new User
                        {
                            UserId = reader["UserId"]?.ToString()??"",
                            RoleId = Convert.ToInt32(reader["RoleId"]),
                            FullName = reader["FullName"]?.ToString()??"",
                            Email = reader["Email"]?.ToString()??"",
                            PasswordHash = reader["PasswordHash"]?.ToString()??"",
                            Phone = reader["Phone"]?.ToString()??"",
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            Role = reader["Role"]?.ToString()??""
                        };
                        return Task.FromResult<User?>(user);
                    }
                }
            }
            catch
            {
                throw;
            }
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByIdAsync(Guid id)
        {
            //throw new NotImplementedException();
            try
            {
                using var connection = new SqlConnection(sqlConnectionString);
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Users WHERE UserId = @UserId";
                command.Parameters.AddWithValue("@UserId", id.ToString());
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var user = new User
                        {
                            UserId = reader["UserId"]?.ToString()??"",
                            RoleId = Convert.ToInt32(reader["RoleId"]),
                            FullName = reader["FullName"]?.ToString()??"",
                            Email = reader["Email"]?.ToString()??"",
                            PasswordHash = reader["PasswordHash"]?.ToString()??"",
                            Phone = reader["Phone"]?.ToString()??"",
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            Role = reader["Role"]?.ToString()??""
                        };
                        return Task.FromResult<User?>(user);
                    }
                }
            }
            catch
            {
                throw;
            }
            return Task.FromResult<User?>(null);
        }

        public Task UpdateAsync(User user)
        {
            //throw new NotImplementedException();
            try
            {
                using var connection = new SqlConnection(sqlConnectionString);
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Users SET FullName = @FullName, Email = @Email, PasswordHash = @PasswordHash, Phone = @Phone, RoleId = @RoleId, IsActive = @IsActive WHERE UserId = @UserId";
                command.Parameters.AddWithValue("@UserId", user.UserId);
                command.Parameters.AddWithValue("@FullName", user.FullName);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@Phone", user.Phone as object ?? DBNull.Value);
                command.Parameters.AddWithValue("@RoleId", user.RoleId);
                command.Parameters.AddWithValue("@IsActive", user.IsActive);

                connection.Open();
                if (command.ExecuteNonQuery() != 1)
                    throw new KeyNotFoundException("The user was not found.");
            }
            catch
            {
                throw;
            }
            return Task.CompletedTask;
        }
        
        public Task<bool> ChaangePasswordAsync(Guid userId, string newPasswordHash)
        {
            try
            {
                using var connection = new SqlConnection(sqlConnectionString);
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserId = @UserId";
                command.Parameters.AddWithValue("@UserId", userId.ToString());
                command.Parameters.AddWithValue("@PasswordHash", newPasswordHash);

                connection.Open();
                var affectedRows = command.ExecuteNonQuery();
                return Task.FromResult(affectedRows == 1);
            }
            catch
            {
                throw;
            }
        }

        public Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            using var connection = new SqlConnection(sqlConnectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO RefreshTokens (TokenHash, UserId, ExpiresAt) VALUES (@TokenHash, @UserId, @ExpiresAt)";
            command.Parameters.AddWithValue("@TokenHash", refreshToken.TokenHash);
            command.Parameters.AddWithValue("@UserId", refreshToken.UserId);
            command.Parameters.AddWithValue("@ExpiresAt", refreshToken.ExpiresAt);
            connection.Open(); command.ExecuteNonQuery();
            return Task.CompletedTask;
        }

        public Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash)
        {
            using var connection = new SqlConnection(sqlConnectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT TokenHash, UserId, ExpiresAt, RevokedAt FROM RefreshTokens WHERE TokenHash = @TokenHash";
            command.Parameters.AddWithValue("@TokenHash", tokenHash);
            connection.Open();
            using var reader = command.ExecuteReader();
            if (!reader.Read()) return Task.FromResult<RefreshToken?>(null);
            return Task.FromResult<RefreshToken?>(new RefreshToken
            {
                TokenHash = reader["TokenHash"].ToString()!, UserId = reader["UserId"].ToString()!,
                ExpiresAt = Convert.ToDateTime(reader["ExpiresAt"]),
                RevokedAt = reader["RevokedAt"] == DBNull.Value ? null : Convert.ToDateTime(reader["RevokedAt"])
            });
        }

        public Task RevokeRefreshTokenAsync(string tokenHash, DateTime revokedAt)
        {
            using var connection = new SqlConnection(sqlConnectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE RefreshTokens SET RevokedAt = @RevokedAt WHERE TokenHash = @TokenHash AND RevokedAt IS NULL";
            command.Parameters.AddWithValue("@TokenHash", tokenHash);
            command.Parameters.AddWithValue("@RevokedAt", revokedAt);
            connection.Open(); command.ExecuteNonQuery();
            return Task.CompletedTask;
        }
    }
}
