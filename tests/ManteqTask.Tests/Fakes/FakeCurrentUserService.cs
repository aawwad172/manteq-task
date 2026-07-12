using ManteqTask.Domain.Interfaces.Application.Services;

namespace ManteqTask.Tests.Fakes;

/// <summary>
/// In-memory <see cref="ICurrentUserService"/> so tests control the acting user id and permissions
/// without an HttpContext.
/// </summary>
internal sealed class FakeCurrentUserService : ICurrentUserService
{
    private readonly HashSet<string> _permissions;

    public FakeCurrentUserService(Guid userId, params string[] permissions)
    {
        UserId = userId;
        _permissions = new HashSet<string>(permissions);
    }

    public Guid UserId { get; set; }

    public bool HasPermission(string permission) => _permissions.Contains(permission);
}
