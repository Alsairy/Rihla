using Microsoft.AspNetCore.Http;
using Rihla.Application.Interfaces;
using System.Security.Claims;

namespace Rihla.WebAPI.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetTenantId()
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id");
            if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out int tenantId))
            {
                return tenantId;
            }
            return 1; // Default to 1 if not found or invalid
        }

        public int GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0; // Default if not found or invalid
        }

        public string GetUserEmail()
        {
            var emailClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email);
            return emailClaim?.Value ?? string.Empty;
        }

        public string GetUserRole()
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value ?? string.Empty;
        }

        public string GetUsername()
        {
            var usernameClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("username");
            return usernameClaim?.Value ?? string.Empty;
        }

        public ClaimsPrincipal GetCurrentUser()
        {
            return _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }
}
