namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of "give or remove the ✓ badge". Same as the API's <c>VerifiedRequest</c>.</summary>
public sealed record VerifiedRequest(bool Verified);
