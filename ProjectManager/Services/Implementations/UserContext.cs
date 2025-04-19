using System.Security.Claims;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Services.Implementations;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null)
                throw new UnauthorizedAccessException("User ID not found in claims");

            return Guid.Parse(userIdClaim.Value);
        }
    }
}
