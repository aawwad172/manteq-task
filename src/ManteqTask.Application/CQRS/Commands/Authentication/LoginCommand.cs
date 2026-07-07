using ManteqTask.Domain.Results;

using MediatR;

namespace ManteqTask.Application.CQRS.Commands.Authentication;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginCommandResult>>;

public record LoginCommandResult(string AccessToken, string RefreshToken);
