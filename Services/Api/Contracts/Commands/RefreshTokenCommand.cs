namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of <c>POST /api/auth/refresh</c>: swaps a refresh token for new tokens.</summary>
public sealed record RefreshTokenCommand(string RefreshToken);
