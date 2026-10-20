namespace Phloem.Services.Api.Contracts;

/// <summary>
/// Body of <c>POST /api/auth/register</c>. Same shape as the API's <c>RegisterCommand</c>.
/// </summary>
/// <remarks>Date of birth and home address are optional at sign-up, but needed later to open a shop or send a
/// parcel. The home address also becomes the "Home" delivery address.</remarks>
public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    Country Country,
    DateOnly? DateOfBirth,
    HomeAddressInput? HomeAddress,
    string Password,
    bool AgreedToTerms);
