namespace ManteqTask.Domain.Interfaces.Application.Services;

public interface ICurrentUserService
{
    Guid UserId { get; set; }
}
