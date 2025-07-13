using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Common;
using SchoolTransportationSystem.Core.Entities;

namespace SchoolTransportationSystem.Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<User>> AuthenticateAsync(string email, string password, string tenantId, string ipAddress, string userAgent);
        Task<Result<User>> GetByIdAsync(int id, string tenantId);
        Task<Result<User>> GetByEmailAsync(string email, string tenantId);
        Task<Result<User>> CreateAsync(CreateUserDto createDto, string tenantId);
        Task<Result<User>> UpdateAsync(int id, UpdateUserDto updateDto, string tenantId);
        Task<Result<bool>> DeleteAsync(int id, string tenantId);
        Task<Result<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string tenantId);
        Task<Result<bool>> UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiryTime, string tenantId);
        Task<Result<User>> ValidateRefreshTokenAsync(string refreshToken, string tenantId);
        Task<Result<bool>> RevokeRefreshTokenAsync(int userId, string tenantId);
    }
}
