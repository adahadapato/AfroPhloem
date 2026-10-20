namespace Phloem.Services.Api;

/// <summary>The outcome of a call that returns a value: the value, or an <see cref="ApiError"/>.</summary>
/// <typeparam name="T">What the API returns.</typeparam>
public sealed class ApiResult<T> : ApiResult
{
    private ApiResult(T? value, ApiError? error) : base(error) => Value = value;

    /// <summary>The API's answer; only meaningful when <see cref="ApiResult.IsSuccess"/>.</summary>
    public T? Value { get; }

    /// <summary>A successful result.</summary>
    public static ApiResult<T> Success(T value) => new(value, null);

    /// <summary>A failed result.</summary>
    public static new ApiResult<T> Failure(ApiError error) => new(default, error);

    /// <summary>Lets <c>return error;</c> be written.</summary>
    public static implicit operator ApiResult<T>(ApiError error) => Failure(error);
}
