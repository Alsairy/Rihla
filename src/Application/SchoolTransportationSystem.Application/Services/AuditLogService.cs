using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Core.Common;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Infrastructure.Data;

namespace SchoolTransportationSystem.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(ApplicationDbContext context, ILogger<AuditLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<bool>> LogEventAsync(int? userId, string email, string action, string ipAddress, string userAgent, bool success, string details, string tenantId)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Email = email,
                    Action = action,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Success = success,
                    Details = details,
                    Timestamp = DateTime.UtcNow,
                    TenantId = tenantId
                };

                _context.Set<AuditLog>().Add(auditLog);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log audit event for user {UserId}, action {Action}", userId, action);
                return Result<bool>.Failure($"Failed to log audit event: {ex.Message}");
            }
        }

        public async Task<Result<bool>> LogLoginAttemptAsync(int? userId, string email, string ipAddress, string userAgent, bool success, string details, string tenantId)
        {
            return await LogEventAsync(userId, email, "Login", ipAddress, userAgent, success, details, tenantId);
        }

        public async Task<Result<bool>> LogLogoutAsync(int userId, string email, string ipAddress, string userAgent, string tenantId)
        {
            return await LogEventAsync(userId, email, "Logout", ipAddress, userAgent, true, "User logged out", tenantId);
        }

        public async Task<Result<bool>> LogPasswordChangeAsync(int userId, string email, string ipAddress, string userAgent, bool success, string tenantId)
        {
            var details = success ? "Password changed successfully" : "Password change failed";
            return await LogEventAsync(userId, email, "PasswordChange", ipAddress, userAgent, success, details, tenantId);
        }

        public async Task<Result<bool>> LogMfaSetupAsync(int userId, string email, string ipAddress, string userAgent, bool success, string tenantId)
        {
            var details = success ? "MFA setup completed" : "MFA setup failed";
            return await LogEventAsync(userId, email, "MfaSetup", ipAddress, userAgent, success, details, tenantId);
        }

        public async Task<Result<bool>> LogMfaVerificationAsync(int userId, string email, string ipAddress, string userAgent, bool success, string tenantId)
        {
            var details = success ? "MFA verification successful" : "MFA verification failed";
            return await LogEventAsync(userId, email, "MfaVerification", ipAddress, userAgent, success, details, tenantId);
        }

        public async Task<Result<bool>> LogAccountLockoutAsync(int userId, string email, string ipAddress, string userAgent, string tenantId)
        {
            return await LogEventAsync(userId, email, "AccountLockout", ipAddress, userAgent, false, "Account locked due to failed login attempts", tenantId);
        }

        public async Task<Result<bool>> LogAccountUnlockAsync(int userId, string email, string ipAddress, string userAgent, string tenantId)
        {
            return await LogEventAsync(userId, email, "AccountUnlock", ipAddress, userAgent, true, "Account unlocked", tenantId);
        }

        public async Task<Result<bool>> LogRoleChangeAsync(int userId, string email, string oldRole, string newRole, string ipAddress, string userAgent, string tenantId)
        {
            var details = $"Role changed from {oldRole} to {newRole}";
            return await LogEventAsync(userId, email, "RoleChange", ipAddress, userAgent, true, details, tenantId);
        }

        public async Task<Result<bool>> LogPermissionDeniedAsync(int? userId, string email, string action, string resource, string ipAddress, string userAgent, string tenantId)
        {
            var details = $"Access denied to {resource} for action {action}";
            return await LogEventAsync(userId, email, "PermissionDenied", ipAddress, userAgent, false, details, tenantId);
        }

        public async Task<Result<List<AuditLog>>> GetAuditLogsAsync(string tenantId, int page = 1, int pageSize = 50, string? userId = null, string? action = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Set<AuditLog>()
                    .Where(a => a.TenantId == tenantId);

                if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt))
                {
                    query = query.Where(a => a.UserId == userIdInt);
                }

                if (!string.IsNullOrEmpty(action))
                {
                    query = query.Where(a => a.Action == action);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp <= endDate.Value);
                }

                var auditLogs = await query
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Result<List<AuditLog>>.Success(auditLogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve audit logs for tenant {TenantId}", tenantId);
                return Result<List<AuditLog>>.Failure($"Failed to retrieve audit logs: {ex.Message}");
            }
        }

        public async Task<Result<int>> GetAuditLogCountAsync(string tenantId, string? userId = null, string? action = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Set<AuditLog>()
                    .Where(a => a.TenantId == tenantId);

                if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt))
                {
                    query = query.Where(a => a.UserId == userIdInt);
                }

                if (!string.IsNullOrEmpty(action))
                {
                    query = query.Where(a => a.Action == action);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp <= endDate.Value);
                }

                var count = await query.CountAsync();
                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to count audit logs for tenant {TenantId}", tenantId);
                return Result<int>.Failure($"Failed to count audit logs: {ex.Message}");
            }
        }

        public async Task<Result<List<AuditLog>>> GetSecurityEventsAsync(string tenantId, int page = 1, int pageSize = 50)
        {
            try
            {
                var securityActions = new[] { "Login", "Logout", "PasswordChange", "MfaSetup", "MfaVerification", "AccountLockout", "AccountUnlock", "PermissionDenied" };

                var securityEvents = await _context.Set<AuditLog>()
                    .Where(a => a.TenantId == tenantId && securityActions.Contains(a.Action))
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Result<List<AuditLog>>.Success(securityEvents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve security events for tenant {TenantId}", tenantId);
                return Result<List<AuditLog>>.Failure($"Failed to retrieve security events: {ex.Message}");
            }
        }
    }
}
