namespace Phloem.Services.Api.Contracts;

/// <summary>Body of <c>POST /api/auth/logout</c>: the refresh token to cancel.</summary>
public sealed record RevokeTokenCommand(string RefreshToken);
