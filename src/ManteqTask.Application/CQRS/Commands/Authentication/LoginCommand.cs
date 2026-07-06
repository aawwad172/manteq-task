using MediatR;

namespace ManteqTask.Application.CQRS.Commands.Authentication;

public record LoginCommand(string Email, string Password) : IRequest<LoginCommandResult>;

public record LoginCommandResult(string AccessToken, string RefreshToken);
