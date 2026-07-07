using System.Security.Claims;

using ManteqTask.Domain.Entities;
using ManteqTask.Domain.Entities.Authentication;
using ManteqTask.Domain.Results;

namespace ManteqTask.Domain.Interfaces.Application.Services;

public interface IJwtService
{
    Task<string> GenerateAccessTokenAsync(User user);
    RefreshToken CreateRefreshTokenEntity(
        User user,
        Guid tokenFamilyId);
    Task<Result<ClaimsPrincipal>> ValidateToken(string token);
}
