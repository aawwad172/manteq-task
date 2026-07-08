namespace ManteqTask.Domain.Interfaces.Application.Services;

public interface ICurrentUserService
{
    Guid UserId { get; set; }

    /// <summary>
    /// True if the current user holds the given permission. The only place in the app that knows
    /// permissions are carried as JWT claims.
    /// </summary>
    bool HasPermission(string permission);
}
