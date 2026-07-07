namespace ManteqTask.Domain.Results;

/// <summary>
/// Outcome of an operation that can fail with an expected, domain-level <see cref="Error"/>.
/// Use instead of throwing for errors that are part of normal control flow.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot contain an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must contain an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    /// <summary>Lifts a bare <see cref="Error"/> into a failed non-generic result.</summary>
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// A <see cref="Result"/> that carries a <typeparamref name="TValue"/> when successful.
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
        => _value = value;

    /// <summary>The success value. Throws if the result is a failure.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    /// <summary>Lets handlers <c>return value;</c> for success.</summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>Lets handlers <c>return Error.NotFound(...);</c> for failure.</summary>
    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}
