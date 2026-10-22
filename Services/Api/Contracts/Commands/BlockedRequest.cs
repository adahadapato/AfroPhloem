namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of "block or unblock a shop". Same as the API's <c>BlockedRequest</c>.</summary>
public sealed record BlockedRequest(bool Blocked);
