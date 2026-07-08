using ManteqTask.Application.CQRS.Commands.Authentication;
using ManteqTask.Domain.Entities;
using ManteqTask.Domain.Entities.Authentication;
using ManteqTask.Domain.Interfaces.Application.Services;
using ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;
using ManteqTask.Domain.Results;

using MediatR;

using Microsoft.Extensions.Logging;

namespace ManteqTask.Application.CQRS.Commands.Authentication;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginCommandResult>>;

public record LoginCommandResult(string AccessToken, string RefreshToken);

internal sealed class LoginCommandHandler(
    ICurrentUserService currentUserService,
    ILogger<LoginCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    ISecurityService securityService,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtService jwtService) : BaseHandler<LoginCommand, Result<LoginCommandResult>>(currentUserService, logger, unitOfWork)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ISecurityService _securityService = securityService;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IJwtService _jwtService = jwtService;

    public override async Task<Result<LoginCommandResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Expected, read-only validations run before any transaction is opened.
        User? user = await _userRepository.GetUserByEmailAsync(request.Email);
        if (user is null)
            return Error.Unauthenticated("Invalid email or password.");

        if (user.IsActive is false)
            return Error.NotActiveUser($"User {user.Id} is not active");

        if (user.DeletedAt is not null)
            return Error.DeletedUser($"User {user.Id} is deleted");

        if (!_securityService.VerifySecret(
                    secret: request.Password,
                    secretHash: user.PasswordHash
                )
            )
            return Error.Unauthenticated("Invalid email or password");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // All tokens for this session belong to one family ID
            Guid tokenFamilyId = Guid.NewGuid();

            string accessToken = await _jwtService.GenerateAccessTokenAsync(user);

            RefreshToken refreshToken = _jwtService.CreateRefreshTokenEntity(user, tokenFamilyId);

            await _refreshTokenRepository.AddAsync(refreshToken);

            user.RefreshTokens.Add(refreshToken);

            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitAsync();

            return new LoginCommandResult(AccessToken: accessToken, RefreshToken: refreshToken.PlaintextToken);
        }
        catch (Exception ex)
        {
            // Unexpected failure while persisting the session: roll back and let it surface as a 500.
            _logger.LogError(ex, "An error occurred during login.");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
