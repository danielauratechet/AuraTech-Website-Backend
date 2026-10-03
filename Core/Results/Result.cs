using AuratechBackend.Constants;

namespace AuratechBackend.Core.Results;

public class Result
{
    protected Result(bool isSuccess, Error error, IReadOnlyList<ValidationError>? validationErrors = null, int? statusCode = null)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("A successful result cannot contain an error.");
        }

        if (!isSuccess && error == Error.None && (validationErrors is null || validationErrors.Count == 0))
        {
            throw new InvalidOperationException("A failed result must contain an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors ?? [];
        StatusCode = statusCode;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public IReadOnlyList<ValidationError> ValidationErrors { get; }

    public int? StatusCode { get; }

    public static Result Success() => new(true, Error.None, statusCode: StatusCodes.Status200OK);

    public static Result Failure(Error error, int statusCode = StatusCodes.Status400BadRequest)
        => new(false, error, statusCode: statusCode);

    public static Result Failure(string code, string message, int statusCode = StatusCodes.Status400BadRequest)
        => Failure(new Error(code, message), statusCode);

    public static Result NotFound(string message = "Resource not found.")
        => Failure(ErrorCodes.NotFound, message, StatusCodes.Status404NotFound);

    public static Result Conflict(string message)
        => Failure(ErrorCodes.Conflict, message, StatusCodes.Status409Conflict);

    public static Result ValidationFailure(IEnumerable<ValidationError> errors)
        => new(false, new Error(ErrorCodes.Validation, "One or more validation errors occurred."),
            errors.ToList(), StatusCodes.Status400BadRequest);

    public static Result ValidationFailure(string propertyName, string message)
        => ValidationFailure([new ValidationError(propertyName, message)]);
}

public class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value)
        : base(true, Error.None, statusCode: StatusCodes.Status200OK)
    {
        _value = value;
    }

    private Result(Error error, IReadOnlyList<ValidationError>? validationErrors, int statusCode)
        : base(false, error, validationErrors, statusCode)
    {
        _value = default;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static Result<T> Success(T value) => new(value);

    public new static Result<T> Failure(Error error, int statusCode = StatusCodes.Status400BadRequest)
        => new(error, null, statusCode);

    public new static Result<T> Failure(string code, string message, int statusCode = StatusCodes.Status400BadRequest)
        => Failure(new Error(code, message), statusCode);

    public new static Result<T> NotFound(string message = "Resource not found.")
        => Failure(ErrorCodes.NotFound, message, StatusCodes.Status404NotFound);

    public new static Result<T> Conflict(string message)
        => Failure(ErrorCodes.Conflict, message, StatusCodes.Status409Conflict);

    public new static Result<T> ValidationFailure(IEnumerable<ValidationError> errors)
        => new(new Error(ErrorCodes.Validation, "One or more validation errors occurred."),
            errors.ToList(), StatusCodes.Status400BadRequest);

    public static implicit operator Result<T>(T value) => Success(value);
}
