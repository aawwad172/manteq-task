using ManteqTask.Domain.Entities.Authentication;

namespace ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, Guid userId);
}
