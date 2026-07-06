using System.Security.Claims;

using ManteqTask.Domain.Entities;
using ManteqTask.Domain.Entities.Authentication;

namespace ManteqTask.Domain.Interfaces.Application.Services;

public interface IJwtService
{
    Task<string> GenerateAccessTokenAsync(User user);
    RefreshToken CreateRefreshTokenEntity(
        User user,
        Guid tokenFamilyId);
    Task<ClaimsPrincipal> ValidateToken(string token);
}
