using ManteqTask.Application.CQRS.Commands.Authentication;
using ManteqTask.Domain.Entities;
using ManteqTask.Domain.Entities.Authentication;
using ManteqTask.Domain.Interfaces.Application.Services;
using ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;
using ManteqTask.Domain.Results;

using Microsoft.Extensions.Logging;

namespace ManteqTask.Application.CQRS.CommandHandlers.Authentication;

public class LogoutCommandHandler(
    ICurrentUserService currentUserService,
    ILogger<LogoutCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IRefreshTokenRepository refreshTokenRepository,
    ISecurityService securityService,
    IUserRepository userRepository)
    : BaseHandler<LogoutCommand, Result<LogoutCommandResult>>(currentUserService, logger, unitOfWork)
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly ISecurityService _securityService = securityService;
    private readonly IUserRepository _userRepository = userRepository;

    public override async Task<Result<LogoutCommandResult>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // 1. --- Fetch Candidate Token (by Plaintext Token) ---
        // CRITICAL: The repository must find the token by the PLAINTEXT value.
        // The repository will fetch the entity based on the PLAINTEXT token value
        // by verifying the hash against the TokenHash/TokenSalt in the DB.

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Error.Unauthorized("Refresh token is required.");

        // This method must perform the hashing/lookup logic.
        RefreshToken? token = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken!, _currentUser.UserId);

        if (token is null || token.IsRevoked || token.IsExpired)
            return Error.Unauthorized("Session is already invalid or token not found.");


        // 2. --- Token Verification (Crucial Security Check) ---
        // Verify the incoming plaintext token matches the stored HASH/SALT.
        // This is done by the service, using the retrieved token's salt.
        bool isVerified = _securityService.VerifySecret(request.RefreshToken!, token.TokenHash);

        if (!isVerified)
            // This suggests a token collision or tampering, treat as unauthorized.
            return Error.Unauthorized("Invalid refresh token signature.");

        User? user = await _userRepository.GetByIdAsync(_currentUser.UserId);

        if (user is null)
            return Error.NotFound("Cannot find user");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 3. --- Revoke and Audit ---
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = "Manual Logout";

            // --------------------------------------------------------------------------
            // CRITICAL: Rotate the SecurityStamp to invalidate all active Access Tokens
            // The next time an active Access Token is presented, the SecurityStamp check
            // in the JwtService.ValidateToken method will fail.
            // --------------------------------------------------------------------------
            user.SecurityStamp = Guid.NewGuid().ToString();


            // Mark the User as updated (optional, but good practice)
            user.UpdatedAt = DateTime.UtcNow;
            // Update the token entity in the repository.
            _refreshTokenRepository.Update(token!);
            // Persist user changes as well (ensures persistence even if retrieved AsNoTracking)
            _userRepository.Update(user);

            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitAsync();

            return new LogoutCommandResult("Logout successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during logout.");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
