using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of <c>PUT /api/me</c>. Same shape as the API's <c>UpdateProfileCommand</c>.</summary>
public sealed record UpdateProfileCommand(string FirstName, string LastName, Country Country, string? PhoneNumber);
