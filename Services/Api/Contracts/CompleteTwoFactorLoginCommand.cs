namespace Phloem.Services.Api.Contracts;

/// <summary>Body of <c>POST /api/auth/login/two-factor</c>: the challenge from login + the code.</summary>
public sealed record CompleteTwoFactorLoginCommand(string ChallengeToken, string Code, bool IsRecoveryCode);
