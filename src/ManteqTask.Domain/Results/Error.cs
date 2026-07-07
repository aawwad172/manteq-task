namespace ManteqTask.Domain.Results;

/// <summary>
/// An expected, domain-level error carried by a failed <see cref="Result"/>.
/// <para>
/// <see cref="Code"/> is a stable machine-readable identifier (e.g. "NOT_FOUND"),
/// <see cref="Message"/> is human-readable, and <see cref="Type"/> categorizes the
/// error for HTTP mapping in the presentation layer.
/// </para>
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    /// <summary>Sentinel value representing "no error"; used by successful results.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error NotFound(string message, string code = "NOT_FOUND")
        => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string message, string code = "CONFLICT")
        => new(code, message, ErrorType.Conflict);

    public static Error Unauthenticated(string message, string code = "UNAUTHENTICATED")
        => new(code, message, ErrorType.Unauthenticated);

    public static Error Unauthorized(string message, string code = "UNAUTHORIZED")
        => new(code, message, ErrorType.Forbidden);

    public static Error NotActiveUser(string message, string code = "NOT_ACTIVE_USER")
        => new(code, message, ErrorType.Forbidden);

    public static Error DeletedUser(string message, string code = "DELETED_USER")
        => new(code, message, ErrorType.Forbidden);

    public static Error Validation(string message, string code = "VALIDATION_ERROR")
        => new(code, message, ErrorType.Validation);

    /// <summary>Builds a single validation error from many messages (joined), matching the current API shape.</summary>
    public static Error Validation(IEnumerable<string> errors, string code = "VALIDATION_ERROR")
        => new(code, string.Join(", ", errors), ErrorType.Validation);

    public static Error Failure(string message, string code = "UNEXPECTED_ERROR")
        => new(code, message, ErrorType.Failure);
}
