using SchoolTransportationSystem.Core.Common;
using SchoolTransportationSystem.Core.Entities;

namespace SchoolTransportationSystem.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task<Result<bool>> LogEventAsync(int? userId, string email, string action, string ipAddress, string userAgent, bool success, string details, string tenantId);
        Task<Result<bool>> LogLoginAttemptAsync(int? userId, string email, string ipAddress, string userAgent, bool success, string details, string tenantId);
        Task<Result<bool>> LogLogoutAsync(int userId, string email, string ipAddress, string userAgent, string tenantId);
        Task<Result<bool>> LogPasswordChangeAsync(int userId, string email, string ipAddress, string userAgent, bool success, string tenantId);
        Task<Result<bool>> LogMfaSetupAsync(int userId, string email, string ipAddress, string userAgent, bool success, string tenantId);
        Task<Result<bool>> LogMfaVerificationAsync(int userId, string email, string ipAddress, string userAgent, bool success, string tenantId);
        Task<Result<bool>> LogAccountLockoutAsync(int userId, string email, string ipAddress, string userAgent, string tenantId);
        Task<Result<bool>> LogAccountUnlockAsync(int userId, string email, string ipAddress, string userAgent, string tenantId);
        Task<Result<bool>> LogRoleChangeAsync(int userId, string email, string oldRole, string newRole, string ipAddress, string userAgent, string tenantId);
        Task<Result<bool>> LogPermissionDeniedAsync(int? userId, string email, string action, string resource, string ipAddress, string userAgent, string tenantId);
        Task<Result<List<AuditLog>>> GetAuditLogsAsync(string tenantId, int page = 1, int pageSize = 50, string? userId = null, string? action = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<Result<int>> GetAuditLogCountAsync(string tenantId, string? userId = null, string? action = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<Result<List<AuditLog>>> GetSecurityEventsAsync(string tenantId, int page = 1, int pageSize = 50);
    }
}
