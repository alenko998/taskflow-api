namespace TaskFlow.Application.Common.Models;

public class Result<T>
{
    public bool   IsSuccess { get; private set; }
    public T?     Value     { get; private set; }
    public string Error     { get; private set; } = string.Empty;
    public int    StatusCode { get; private set; }

    private Result(bool isSuccess, T? value, string error, int statusCode)
    {
        IsSuccess  = isSuccess;
        Value      = value;
        Error      = error;
        StatusCode = statusCode;
    }

    public static Result<T> Success(T value)                        => new(true,  value,   string.Empty, 200);
    public static Result<T> Created(T value)                        => new(true,  value,   string.Empty, 201);
    public static Result<T> Failure(string error)                   => new(false, default, error,        400);
    public static Result<T> NotFound(string error)                  => new(false, default, error,        404);
    public static Result<T> Unauthorized(string error)              => new(false, default, error,        401);
    public static Result<T> Forbidden(string error)                 => new(false, default, error,        403);
    public static Result<T> Conflict(string error)                  => new(false, default, error,        409);
}

public class Result
{
    public bool   IsSuccess  { get; private set; }
    public string Error      { get; private set; } = string.Empty;
    public int    StatusCode { get; private set; }

    private Result(bool isSuccess, string error, int statusCode)
    {
        IsSuccess  = isSuccess;
        Error      = error;
        StatusCode = statusCode;
    }

    public static Result Success()                   => new(true,  string.Empty, 200);
    public static Result Failure(string error)       => new(false, error,        400);
    public static Result NotFound(string error)      => new(false, error,        404);
    public static Result Unauthorized(string error)  => new(false, error,        401);
    public static Result Forbidden(string error)     => new(false, error,        403);
}