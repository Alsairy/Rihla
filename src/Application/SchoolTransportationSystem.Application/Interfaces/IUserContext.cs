using System.Security.Claims;

namespace Rihla.Application.Interfaces
{
    public interface IUserContext
    {
        string GetTenantId();
        int GetUserId();
        string GetUserEmail();
        string GetUserRole();
        string GetUsername();
        ClaimsPrincipal GetCurrentUser();
        bool IsAuthenticated();
    }
}
