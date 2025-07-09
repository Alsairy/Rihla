using Microsoft.EntityFrameworkCore;
using Rihla.Application.DTOs;
using Rihla.Application.Interfaces;
using Rihla.Core.Common;
using Rihla.Core.Entities;
using Rihla.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Rihla.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<User>> AuthenticateAsync(string email, string password, string tenantId)
        {
            try
            {
                var user = await _unitOfWork.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId && u.IsActive);

                if (user == null)
                {
                    return Result<User>.Failure("Invalid email or password");
                }

                if (!VerifyPassword(password, user.PasswordHash, user.Salt))
                {
                    return Result<User>.Failure("Invalid email or password");
                }

                user.LastLoginAt = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Result<User>.Failure($"Authentication failed: {ex.Message}");
            }
        }

        public async Task<Result<User>> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id, tenantId);

                if (user == null)
                {
                    return Result<User>.Failure("User not found");
                }

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Result<User>.Failure($"Error retrieving user: {ex.Message}");
            }
        }

        public async Task<Result<User>> GetByEmailAsync(string email, string tenantId)
        {
            try
            {
                var user = await _unitOfWork.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId);

                if (user == null)
                {
                    return Result<User>.Failure("User not found");
                }

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Result<User>.Failure($"Error retrieving user: {ex.Message}");
            }
        }

        public async Task<Result<User>> CreateAsync(CreateUserDto createDto, string tenantId)
        {
            try
            {
                var existingUser = await _unitOfWork.Users
                    .FirstOrDefaultAsync(u => (u.Email == createDto.Email || u.Username == createDto.Username) && u.TenantId == tenantId);

                if (existingUser != null)
                {
                    return Result<User>.Failure("User with this email or username already exists");
                }

                var (passwordHash, salt) = HashPassword(createDto.Password);

                var user = new User
                {
                    Username = createDto.Username,
                    Email = createDto.Email,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    Role = createDto.Role,
                    TenantId = tenantId,
                    FirstName = createDto.FirstName,
                    LastName = createDto.LastName,
                    IsActive = createDto.IsActive
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Result<User>.Failure($"Error creating user: {ex.Message}");
            }
        }

        public async Task<Result<User>> UpdateAsync(int id, UpdateUserDto updateDto, string tenantId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id, tenantId);

                if (user == null)
                {
                    return Result<User>.Failure("User not found");
                }

                if (!string.IsNullOrEmpty(updateDto.Username))
                    user.Username = updateDto.Username;
                
                if (!string.IsNullOrEmpty(updateDto.Email))
                    user.Email = updateDto.Email;
                
                if (!string.IsNullOrEmpty(updateDto.Role))
                    user.Role = updateDto.Role;
                
                if (!string.IsNullOrEmpty(updateDto.FirstName))
                    user.FirstName = updateDto.FirstName;
                
                if (!string.IsNullOrEmpty(updateDto.LastName))
                    user.LastName = updateDto.LastName;
                
                user.IsActive = updateDto.IsActive;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Result<User>.Failure($"Error updating user: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, string tenantId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id, tenantId);

                if (user == null)
                {
                    return Result<bool>.Failure("User not found");
                }

                await _unitOfWork.Users.DeleteAsync(user, "System");
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error deleting user: {ex.Message}");
            }
        }

        public async Task<Result<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string tenantId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId, tenantId);

                if (user == null)
                {
                    return Result<bool>.Failure("User not found");
                }

                if (!VerifyPassword(currentPassword, user.PasswordHash, user.Salt))
                {
                    return Result<bool>.Failure("Current password is incorrect");
                }

                var (passwordHash, salt) = HashPassword(newPassword);
                user.PasswordHash = passwordHash;
                user.Salt = salt;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error changing password: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiryTime, string tenantId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId, tenantId);

                if (user == null)
                {
                    return Result<bool>.Failure("User not found");
                }

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = expiryTime;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error updating refresh token: {ex.Message}");
            }
        }

        public async Task<Result<User>> ValidateRefreshTokenAsync(string refreshToken, string tenantId)
        {
            try
            {
                var user = await _unitOfWork.Users
                    .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.TenantId == tenantId && u.IsActive);

                if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return Result<User>.Failure("Invalid or expired refresh token");
                }

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Result<User>.Failure($"Error validating refresh token: {ex.Message}");
            }
        }

        public async Task<Result<bool>> RevokeRefreshTokenAsync(int userId, string tenantId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId, tenantId);

                if (user == null)
                {
                    return Result<bool>.Failure("User not found");
                }

                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error revoking refresh token: {ex.Message}");
            }
        }

        private (string hash, string salt) HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[32];
            rng.GetBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));

            return (hash, salt);
        }

        private bool VerifyPassword(string password, string hash, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            var computedHash = Convert.ToBase64String(pbkdf2.GetBytes(32));

            return computedHash == hash;
        }
    }
}
