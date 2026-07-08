using ManteqTask.Domain.Entities.Authentication;
using ManteqTask.Domain.Interfaces.Application.Services;

using Microsoft.AspNetCore.Http;

namespace ManteqTask.Application.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid UserId { get; set; }

    // Single place that knows permissions live in the JWT claims (CustomClaims.Permission).
    public bool HasPermission(string permission)
        => _httpContextAccessor.HttpContext?.User?.HasClaim(CustomClaims.Permission, permission) ?? false;
}
