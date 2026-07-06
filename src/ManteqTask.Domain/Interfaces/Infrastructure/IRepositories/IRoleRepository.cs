using ManteqTask.Domain.Entities.Authentication;

namespace ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;

public interface IRoleRepository
{
    Task<Role?> GetRoleByNameAsync(string name);
}
