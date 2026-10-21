namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>
/// Body of <c>PUT /api/me/personal-details</c> (needed before opening a shop or sending a parcel).
/// Same shape as the API's <c>UpdatePersonalDetailsCommand</c>.
/// </summary>
public sealed record UpdatePersonalDetailsCommand(DateOnly DateOfBirth, HomeAddressInput HomeAddress);
