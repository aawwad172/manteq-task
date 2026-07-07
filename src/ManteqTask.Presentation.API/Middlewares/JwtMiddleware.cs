using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using ManteqTask.Domain.Interfaces.Application.Services;
using ManteqTask.Domain.Results;

namespace ManteqTask.Presentation.API.Middlewares;

/// <summary>
/// Middleware for validating JWT tokens and attaching user information to the HTTP context.
/// </summary>
/// <remarks>
/// Make sure to update the configuration settings for "Jwt:JwtSecretKey", "Jwt:JwtIssuer", and "Jwt:JwtAudience" as needed.
/// Visit https://jwtsecret.com/generate for generating a secure JWT secret key.
/// </remarks>
public class JwtMiddleware(
    RequestDelegate next,
    ILogger<JwtMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<JwtMiddleware> _logger = logger;

    /// <summary>
    /// Invokes the middleware to validate the JWT token from the request header and attach the user to the context.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task Invoke(HttpContext context, ICurrentUserService currentUser)
    {
        IJwtService _jwtService = context.RequestServices.GetRequiredService<IJwtService>();

        string? token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(token))
        {
            await _next(context);
            return;
        }

        try
        {
            Result<ClaimsPrincipal> result = await _jwtService.ValidateToken(token!);

            if (result.IsSuccess)
            {
                ClaimsPrincipal principal = result.Value;
                context.User = principal;


                // Try the short alias ('nameid') which is the standard JWT name for the ID.
                string? userId = principal.FindFirst(JwtRegisteredClaimNames.NameId)?.Value
                                 // Fallback to other common aliases if needed
                                 ?? principal.FindFirst("sub")?.Value
                                 ?? principal.FindFirst("id")?.Value;

                // If you want to keep the .NET URI constant, ensure you check that too:
                userId ??= principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    currentUser.UserId = Guid.Parse(userId);
                }
            }
            else if (result.Error.Code == "INVALID_TOKEN")
            {
                // Token is expired/invalid but we can still extract claims for refresh flow
                try
                {
                    // Read token without validation to extract claims
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);

                    string? userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                                     ?? jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)?.Value
                                     ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "id")?.Value
                                     ?? jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(userId))
                    {
                        currentUser.UserId = Guid.Parse(userId);
                        _logger.LogDebug("Extracted userId from expired token for refresh flow");
                    }
                }
                catch (Exception readEx)
                {
                    _logger.LogWarning(readEx, "Failed to extract claims from invalid token");
                }
            }
            else
            {
                _logger.LogWarning("Authentication failed: {Error}", result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate JWT token");
        }
        await _next(context);
    }
}
