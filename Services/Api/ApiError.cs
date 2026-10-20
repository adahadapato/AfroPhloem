namespace Phloem.Services.Api;

/// <summary>
/// Why a request failed, read from the API's ProblemDetails answer (or made up here when the API couldn't be
/// reached). Show <see cref="DisplayMessage"/> to the user; check <see cref="Code"/> in code.
/// </summary>
/// <param name="Status">HTTP status (0 when the API wasn't reached).</param>
/// <param name="Code">The API's error code, e.g. "Auth.InvalidCredentials", "UserProfile.PersonalDetailsRequired".</param>
/// <param name="Message">The API's explanation, ready to show.</param>
/// <param name="FieldErrors">Validation problems per field (empty when there are none).</param>
public sealed record ApiError(
    int Status,
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]> FieldErrors)
{
    private static readonly IReadOnlyDictionary<string, string[]> NoFieldErrors = new Dictionary<string, string[]>();

    /// <summary>The API couldn't be reached (no internet, server down, wrong address).</summary>
    public static ApiError Unreachable { get; } = new(0, "Network.Unreachable",
        "We couldn't reach Phloem. Please check your internet connection and try again.", NoFieldErrors);

    /// <summary>The API didn't answer in time.</summary>
    public static ApiError Timeout { get; } = new(0, "Network.Timeout",
        "Phloem is taking too long to answer. Please try again.", NoFieldErrors);

    /// <summary>The sign-in ran out and couldn't be renewed: the user must sign in again.</summary>
    public static ApiError SessionExpired { get; } = new(401, "Auth.SessionExpired",
        "Your session has ended. Please sign in again.", NoFieldErrors);

    /// <summary>Anything else that went wrong without a usable answer.</summary>
    public static ApiError Unexpected(int status) => new(status, "Api.Unexpected",
        "Something went wrong. Please try again.", NoFieldErrors);

    /// <summary>The message for the user: every field problem (one per line), or the general message.</summary>
    public string DisplayMessage => FieldErrors.Count == 0
        ? Message
        : string.Join(Environment.NewLine, FieldErrors.SelectMany(f => f.Value).Distinct());

    /// <summary>True when the user isn't (or is no longer) signed in.</summary>
    public bool IsUnauthorized => Status == 401;

    /// <summary>True when the API couldn't be reached at all.</summary>
    public bool IsNetworkError => Status == 0;
}
