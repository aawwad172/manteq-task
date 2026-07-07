using ManteqTask.Application.CQRS.Commands.Authentication;
using ManteqTask.Domain.Entities;
using ManteqTask.Domain.Entities.Authentication;
using ManteqTask.Domain.Interfaces.Application.Services;
using ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;
using ManteqTask.Domain.Results;

using Microsoft.Extensions.Logging;

namespace ManteqTask.Application.CQRS.CommandHandlers.Authentication;

public class RefreshTokenCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<RefreshTokenCommandHandler> logger,
    ICurrentUserService currentUserService,
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IJwtService jwtService)
    : BaseHandler<RefreshTokenCommand, Result<RefreshTokenCommandResult>>(currentUserService, logger, unitOfWork)
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IJwtService _jwtService = jwtService;

    public override async Task<Result<RefreshTokenCommandResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        RefreshToken? oldToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, _currentUser.UserId);

        if (oldToken is null || !oldToken.IsActive)
            return Error.Unauthenticated("Invalid or expired refresh token.");


        User? user = await _userRepository.GetByIdAsync(oldToken.UserId);
        if (user is null)
            return Error.NotFound("User not found");

        if (oldToken.SecurityStampAtIssue != user.SecurityStamp)
            return Error.Unauthenticated("Session security has been compromised. Please log in again.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            //  Generate a new refresh token
            RefreshToken newRefreshToken = _jwtService.CreateRefreshTokenEntity(user, oldToken.TokenFamilyId);

            oldToken.RevokedAt = DateTime.UtcNow;
            oldToken.ReasonRevoked = "Rotated";
            oldToken.ReplacedByTokenId = newRefreshToken.Id; // CRITICAL: Link to the new token ID

            _refreshTokenRepository.Update(oldToken);

            // Add the new refresh token to the repository
            await _refreshTokenRepository.AddAsync(newRefreshToken);

            // Generate a new access token with desired expiration
            string newAccessToken = await _jwtService.GenerateAccessTokenAsync(user);

            // Save all changes and commit the transaction
            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitAsync();

            // 4. --- Return New Tokens ---
            // newRefreshTokenEntity.PlaintextToken is available because of the [NotMapped] property.
            return new RefreshTokenCommandResult(newAccessToken, newRefreshToken.PlaintextToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred during rotating token: {Message}", ex.Message);
            await _unitOfWork.RollbackAsync();
            throw;
        }

    }
}
