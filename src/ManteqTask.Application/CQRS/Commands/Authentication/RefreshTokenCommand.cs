using ManteqTask.Domain.Results;

using MediatR;

namespace ManteqTask.Application.CQRS.Commands.Authentication;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<RefreshTokenCommandResult>>;

public record RefreshTokenCommandResult(string AccessToken, string RefreshToken);
