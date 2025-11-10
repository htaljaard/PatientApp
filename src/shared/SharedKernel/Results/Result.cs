using System;

namespace PatientApp.SharedKernel.Results;
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public T? Value { get; }

    private Result(bool isSuccess, T? value, Error? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("A successful result cannot have an error message.");
        if (!isSuccess && error is null)
            throw new InvalidOperationException("A failure result must have an error message.");

        IsSuccess = isSuccess;
        Error = error;
        Value = value;
    }

    public static Result<T> Success(T value) => new Result<T>(true, value, null);

    public static Result<T> Failure(string error) => new Result<T>(false, default, new Error(error));

    public static Result<T> Failure(Error error) => new Result<T>(false, default, error);

    /// <summary>
    /// Implicitly returns the underlying value when the result is successful.
    /// Throws InvalidOperationException if the result is a failure.
    /// </summary>
    public static implicit operator T(Result<T> result)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));
        if (result.IsFailure) throw new InvalidOperationException("Cannot get value from a failed Result.");
        return result.Value!;
    }

    /// <summary>
    /// Allow implicitly converting a value of type T to a successful Result&lt;T&gt;.
    /// This enables returning the raw value from a method whose return type is Result&lt;T&gt; without calling Success explicitly.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Allow implicitly converting an Error instance to a failed Result&lt;T&gt;.
    /// This enables returning an Error directly from a method whose return type is Result&lt;T&gt;.
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);

    public override string ToString()
    {
        return IsSuccess ? $"Success: {Value}" : $"Failure: {Error}";
    }
}

/// <summary>
/// Non-generic helper to make it easier to create Result&lt;T&gt; instances without specifying the generic type at the call site.
/// Type argument can be inferred from the assignment target.
/// Example: `Result<PrivateHealthFundAccount> r = Result.Failure("msg");`
/// </summary>
public static class Result
{
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}
