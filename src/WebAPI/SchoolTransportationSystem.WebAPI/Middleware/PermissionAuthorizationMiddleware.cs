using Microsoft.EntityFrameworkCore;
using SchoolTransportationSystem.Infrastructure.Data;
using System.Security.Claims;

namespace SchoolTransportationSystem.WebAPI.Middleware
{
    public class PermissionAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionAuthorizationMiddleware> _logger;

        public PermissionAuthorizationMiddleware(RequestDelegate next, ILogger<PermissionAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            if (ShouldSkipAuthorization(context.Request.Path))
            {
                await _next(context);
                return;
            }

            if (!context.User.Identity?.IsAuthenticated == true)
            {
                await _next(context);
                return;
            }

            try
            {
                var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                var tenantId = context.User.FindFirst("tenant_id")?.Value;
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(tenantId))
                {
                    _logger.LogWarning("User missing role or tenant information");
                    await _next(context);
                    return;
                }

                var resource = GetResourceFromPath(context.Request.Path);
                var action = GetActionFromMethod(context.Request.Method);

                var hasPermission = await CheckPermissionAsync(dbContext, userRole, resource, action, tenantId);

                if (!hasPermission)
                {
                    _logger.LogWarning("Permission denied for user {UserId}, role {Role}, resource {Resource}, action {Action}", 
                        userId, userRole, resource, action);

                    await LogPermissionDeniedAsync(dbContext, userId, context.User.FindFirst(ClaimTypes.Email)?.Value ?? "", 
                        action, resource, GetClientIpAddress(context), GetUserAgent(context), tenantId);

                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Access denied: Insufficient permissions");
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in permission authorization middleware");
                await _next(context);
            }
        }

        private bool ShouldSkipAuthorization(PathString path)
        {
            var skipPaths = new[]
            {
                "/api/auth/login",
                "/api/auth/refresh",
                "/swagger",
                "/",
                "/test-db",
                "/seed-db",
                "/notificationHub"
            };

            return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
        }

        private string GetResourceFromPath(PathString path)
        {
            var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments?.Length >= 2 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
            {
                return segments[1].ToLowerInvariant();
            }
            return "unknown";
        }

        private string GetActionFromMethod(string method)
        {
            return method.ToUpperInvariant() switch
            {
                "GET" => "Read",
                "POST" => "Create",
                "PUT" => "Update",
                "PATCH" => "Update",
                "DELETE" => "Delete",
                _ => "Unknown"
            };
        }

        private async Task<bool> CheckPermissionAsync(ApplicationDbContext dbContext, string role, string resource, string action, string tenantId)
        {
            try
            {
                var hasPermission = await dbContext.Set<SchoolTransportationSystem.Core.Entities.RolePermission>()
                    .Include(rp => rp.Permission)
                    .AnyAsync(rp => rp.Role == role &&
                                   rp.Permission.Resource == resource &&
                                   rp.Permission.Action == action &&
                                   rp.Permission.TenantId == tenantId &&
                                   rp.Permission.IsActive &&
                                   rp.IsGranted);

                if (hasPermission)
                    return true;

                var hasWildcardPermission = await dbContext.Set<SchoolTransportationSystem.Core.Entities.RolePermission>()
                    .Include(rp => rp.Permission)
                    .AnyAsync(rp => rp.Role == role &&
                                   (rp.Permission.Resource == "All" || rp.Permission.Resource == "*") &&
                                   rp.Permission.TenantId == tenantId &&
                                   rp.Permission.IsActive &&
                                   rp.IsGranted);

                if (hasWildcardPermission)
                    return true;

                return CheckDefaultRolePermissions(role, resource, action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permissions for role {Role}, resource {Resource}, action {Action}", role, resource, action);
                return CheckDefaultRolePermissions(role, resource, action);
            }
        }

        private bool CheckDefaultRolePermissions(string role, string resource, string action)
        {
            return role switch
            {
                "SuperAdmin" or "TenantAdmin" => true, // Full access
                "SystemAdmin" => IsSystemAdminResource(resource),
                "SafetyOfficer" => IsSafetyOfficerResource(resource),
                "Dispatcher" => IsDispatcherResource(resource),
                "Driver" => IsDriverResource(resource, action),
                "Parent" => IsParentResource(resource, action),
                "Student" => IsStudentResource(resource, action),
                _ => false
            };
        }

        private bool IsSystemAdminResource(string resource)
        {
            var allowedResources = new[] { "users", "drivers", "vehicles", "routes", "trips", "attendance", "reports" };
            return allowedResources.Contains(resource);
        }

        private bool IsSafetyOfficerResource(string resource)
        {
            var allowedResources = new[] { "drivers", "vehicles", "routes", "trips", "attendance", "maintenance", "reports" };
            return allowedResources.Contains(resource);
        }

        private bool IsDispatcherResource(string resource)
        {
            var allowedResources = new[] { "drivers", "vehicles", "routes", "trips", "attendance" };
            return allowedResources.Contains(resource);
        }

        private bool IsDriverResource(string resource, string action)
        {
            if (resource == "trips" || resource == "attendance")
                return action == "Read" || action == "Update";
            if (resource == "drivers")
                return action == "Read";
            return false;
        }

        private bool IsParentResource(string resource, string action)
        {
            var allowedResources = new[] { "students", "trips", "attendance", "payments" };
            return allowedResources.Contains(resource) && (action == "Read" || (resource == "payments" && action == "Create"));
        }

        private bool IsStudentResource(string resource, string action)
        {
            var allowedResources = new[] { "trips", "attendance" };
            return allowedResources.Contains(resource) && action == "Read";
        }

        private async Task LogPermissionDeniedAsync(ApplicationDbContext dbContext, string? userId, string email, 
            string action, string resource, string ipAddress, string userAgent, string tenantId)
        {
            try
            {
                var auditLog = new SchoolTransportationSystem.Core.Entities.AuditLog
                {
                    UserId = int.TryParse(userId, out int userIdInt) ? userIdInt : null,
                    Email = email,
                    Action = "PermissionDenied",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Success = false,
                    Details = $"Access denied to {resource} for action {action}",
                    Timestamp = DateTime.UtcNow,
                    TenantId = tenantId
                };

                dbContext.Set<SchoolTransportationSystem.Core.Entities.AuditLog>().Add(auditLog);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log permission denied event");
            }
        }

        private string GetClientIpAddress(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private string GetUserAgent(HttpContext context)
        {
            return context.Request.Headers["User-Agent"].ToString();
        }
    }
}
