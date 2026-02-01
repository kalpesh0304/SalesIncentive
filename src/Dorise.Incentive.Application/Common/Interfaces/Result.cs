namespace Dorise.Incentive.Application.Common.Interfaces;

/// <summary>
/// Represents the result of an operation.
/// "I ated the purple berries!" - Results tell you what happened after!
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? error, string? errorCode)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Success result cannot have an error.");

        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, string? errorCode = null) => new(false, error, errorCode);
    public static Result<T> Success<T>(T value) => new(value, true, null, null);
    public static Result<T> Failure<T>(string error, string? errorCode = null) => new(default, false, error, errorCode);

    public static Result NotFound(string entityName, object id) =>
        Failure($"{entityName} with ID '{id}' was not found.", "NOT_FOUND");

    public static Result<T> NotFound<T>(string entityName, object id) =>
        Failure<T>($"{entityName} with ID '{id}' was not found.", "NOT_FOUND");

    public static Result ValidationError(string message) =>
        Failure(message, "VALIDATION_ERROR");

    public static Result<T> ValidationError<T>(string message) =>
        Failure<T>(message, "VALIDATION_ERROR");
}

/// <summary>
/// Represents the result of an operation that returns a value.
/// </summary>
/// <typeparam name="T">The type of value</typeparam>
public class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, string? error, string? errorCode)
        : base(isSuccess, error, errorCode)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, null, null);
    public static new Result<T> Failure(string error, string? errorCode = null) => new(default, false, error, errorCode);
    public static new Result<T> NotFound(string entityName, object id) =>
        Failure($"{entityName} with ID '{id}' was not found.", "NOT_FOUND");
    public static new Result<T> ValidationError(string message) =>
        Failure(message, "VALIDATION_ERROR");

    public static implicit operator Result<T>(T value) => Success(value);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Error!);
    }
}

/// <summary>
/// Paged result for queries that return lists.
/// "Go banana!" - And go fetch those pages!
/// </summary>
/// <typeparam name="T">The type of items</typeparam>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    public static PagedResult<T> Empty(int page = 1, int pageSize = 20) =>
        new(Array.Empty<T>(), 0, page, pageSize);
}
