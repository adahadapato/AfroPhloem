namespace Phloem.Services.Api.Contracts;

/// <summary>Body of <c>POST /api/auth/login</c>.</summary>
public sealed record LoginCommand(string Email, string Password);
