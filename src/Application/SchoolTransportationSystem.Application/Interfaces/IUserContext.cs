using System.Security.Claims;

namespace SchoolTransportationSystem.Application.Interfaces
{
    public interface IUserContext
    {
        int GetTenantId();
        int GetUserId();
        string GetUserEmail();
        string GetUserRole();
        string GetUsername();
        ClaimsPrincipal GetCurrentUser();
        bool IsAuthenticated();
    }
}
