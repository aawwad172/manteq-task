using ManteqTask.Domain.Results;
using ManteqTask.Presentation.API.Models;

namespace ManteqTask.Presentation.API.Extensions;

/// <summary>
/// Maps a domain <see cref="Result{T}"/> / <see cref="Error"/> onto the HTTP boundary:
/// success becomes a caller-supplied <see cref="IResult"/>, failure becomes an
/// <see cref="ApiResponse{T}"/> error payload with the mapped status code.
/// </summary>
public static class ResultExtensions
{
    public static IResult ToApiResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
        => result.IsSuccess ? onSuccess(result.Value) : result.Error.ToErrorResult();

    public static IResult ToErrorResult(this Error error)
    {
        int statusCode = error.Type.ToStatusCode();
        ApiResponse<string> response = ApiResponse<string>.ErrorResponse(error.Message, error.Code, statusCode);
        return Results.Json(response, statusCode: statusCode);
    }

    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthenticated => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };
}
