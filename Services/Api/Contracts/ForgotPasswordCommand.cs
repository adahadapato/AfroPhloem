namespace Phloem.Services.Api.Contracts;

/// <summary>Body of <c>POST /api/auth/forgot-password</c>.</summary>
public sealed record ForgotPasswordCommand(string Email);
