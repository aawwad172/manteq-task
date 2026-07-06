using ManteqTask.Domain.Interfaces.Application.Services;

namespace ManteqTask.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    public Guid UserId { get; set; }
}
