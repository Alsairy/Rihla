using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace SchoolTransportationSystem.WebAPI.Attributes
{
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(string permission)
        {
            Policy = $"Permission:{permission}";
        }
    }

    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PermissionRequirementHandler> _logger;

        public PermissionRequirementHandler(IServiceProvider serviceProvider, ILogger<PermissionRequirementHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (!context.User.Identity?.IsAuthenticated == true)
            {
                _logger.LogWarning("User not authenticated for permission {Permission}", requirement.Permission);
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SchoolTransportationSystem.Infrastructure.Data.ApplicationDbContext>();

                var userRole = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var tenantId = context.User.FindFirst("tenant_id")?.Value;
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(tenantId))
                {
                    _logger.LogWarning("User missing role or tenant information for permission {Permission}", requirement.Permission);
                    return;
                }

                var hasPermission = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(
                    dbContext.Set<SchoolTransportationSystem.Core.Entities.RolePermission>()
                        .Include(rp => rp.Permission),
                    rp => rp.Role == userRole &&
                          rp.Permission.Name == requirement.Permission &&
                          rp.Permission.TenantId == tenantId &&
                          rp.Permission.IsActive &&
                          rp.IsGranted);

                if (hasPermission)
                {
                    context.Succeed(requirement);
                    return;
                }

                var hasWildcardPermission = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(
                    dbContext.Set<SchoolTransportationSystem.Core.Entities.RolePermission>()
                        .Include(rp => rp.Permission),
                    rp => rp.Role == userRole &&
                          (rp.Permission.Name == "All" || rp.Permission.Name == "*") &&
                          rp.Permission.TenantId == tenantId &&
                          rp.Permission.IsActive &&
                          rp.IsGranted);

                if (hasWildcardPermission)
                {
                    context.Succeed(requirement);
                    return;
                }

                if (CheckDefaultRolePermissions(userRole, requirement.Permission))
                {
                    context.Succeed(requirement);
                    return;
                }

                _logger.LogWarning("Permission {Permission} denied for user {UserId} with role {Role}", 
                    requirement.Permission, userId, userRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {Permission}", requirement.Permission);
            }
        }

        private bool CheckDefaultRolePermissions(string role, string permission)
        {
            return role switch
            {
                "SuperAdmin" or "TenantAdmin" => true, // Full access
                "SystemAdmin" => IsSystemAdminPermission(permission),
                "SafetyOfficer" => IsSafetyOfficerPermission(permission),
                "Dispatcher" => IsDispatcherPermission(permission),
                "Driver" => IsDriverPermission(permission),
                "Parent" => IsParentPermission(permission),
                "Student" => IsStudentPermission(permission),
                _ => false
            };
        }

        private bool IsSystemAdminPermission(string permission)
        {
            var allowedPermissions = new[]
            {
                "ManageUsers", "ViewUsers", "ManageDrivers", "ViewDrivers",
                "ManageVehicles", "ViewVehicles", "ManageRoutes", "ViewRoutes",
                "ManageTrips", "ViewTrips", "ViewAttendance", "ViewReports"
            };
            return allowedPermissions.Contains(permission);
        }

        private bool IsSafetyOfficerPermission(string permission)
        {
            var allowedPermissions = new[]
            {
                "ViewDrivers", "ViewVehicles", "ViewRoutes", "ViewTrips",
                "ViewAttendance", "ManageMaintenance", "ViewMaintenance", "ViewReports"
            };
            return allowedPermissions.Contains(permission);
        }

        private bool IsDispatcherPermission(string permission)
        {
            var allowedPermissions = new[]
            {
                "ViewDrivers", "ViewVehicles", "ViewRoutes", "ManageTrips",
                "ViewTrips", "ManageAttendance", "ViewAttendance"
            };
            return allowedPermissions.Contains(permission);
        }

        private bool IsDriverPermission(string permission)
        {
            var allowedPermissions = new[]
            {
                "ViewTrips", "UpdateAttendance", "ViewAttendance"
            };
            return allowedPermissions.Contains(permission);
        }

        private bool IsParentPermission(string permission)
        {
            var allowedPermissions = new[]
            {
                "ViewStudents", "ViewTrips", "ViewAttendance", "ManagePayments", "ViewPayments"
            };
            return allowedPermissions.Contains(permission);
        }

        private bool IsStudentPermission(string permission)
        {
            var allowedPermissions = new[]
            {
                "ViewTrips", "ViewAttendance"
            };
            return allowedPermissions.Contains(permission);
        }
    }
}
