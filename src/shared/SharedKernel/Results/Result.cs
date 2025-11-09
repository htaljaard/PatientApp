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

    public override string ToString()
    {
        return IsSuccess ? $"Success: {Value}" : $"Failure: {Error}";
    }
}
