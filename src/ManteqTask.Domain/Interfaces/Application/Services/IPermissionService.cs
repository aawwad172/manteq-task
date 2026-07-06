using ManteqTask.Domain.Entities;

namespace ManteqTask.Domain.Interfaces.Application.Services;

public interface IPermissionService
{
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<List<string>> GetUserPermissionsAsync(User user);
}
