using ManteqTask.Domain.Results;

using MediatR;

namespace ManteqTask.Application.CQRS.Commands.Authentication;

public record LogoutCommand(string? RefreshToken) : IRequest<Result<LogoutCommandResult>>;

public record LogoutCommandResult(string Message);
