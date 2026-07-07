namespace ManteqTask.Domain.Results;

/// <summary>
/// Categorizes an <see cref="Error"/> so the presentation layer can map it to an
/// HTTP status code without the domain having any knowledge of HTTP.
/// </summary>
public enum ErrorType
{
    /// <summary>Unexpected / unhandled failure. Maps to 500.</summary>
    Failure = 0,

    /// <summary>Input failed validation. Maps to 400.</summary>
    Validation,

    /// <summary>Caller is not authenticated. Maps to 401.</summary>
    Unauthenticated,

    /// <summary>Caller is authenticated but not permitted. Maps to 403.</summary>
    Forbidden,

    /// <summary>Requested resource does not exist. Maps to 404.</summary>
    NotFound,

    /// <summary>Request conflicts with current resource state. Maps to 409.</summary>
    Conflict,
}
