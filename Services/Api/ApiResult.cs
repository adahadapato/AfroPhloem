namespace Phloem.Services.Api;

/// <summary>
/// The outcome of a call that returns nothing: success, or an <see cref="ApiError"/>.
/// Mirrors the API's own <c>Result</c>, so screens never deal with HTTP or exceptions.
/// </summary>
public class ApiResult
{
    /// <summary>Creates a result; use <see cref="Success"/> or an <see cref="ApiError"/>.</summary>
    protected ApiResult(ApiError? error) => Error = error;

    /// <summary>Why it failed; null on success.</summary>
    public ApiError? Error { get; }

    /// <summary>True when the call worked.</summary>
    public bool IsSuccess => Error is null;

    /// <summary>True when the call failed (see <see cref="Error"/>).</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>A successful result.</summary>
    public static ApiResult Success() => new(null);

    /// <summary>A failed result.</summary>
    public static ApiResult Failure(ApiError error) => new(error);

    /// <summary>Lets <c>return error;</c> be written instead of <c>ApiResult.Failure(error)</c>.</summary>
    public static implicit operator ApiResult(ApiError error) => Failure(error);
}
