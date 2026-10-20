namespace Phloem.Services.Api.Contracts;

/// <summary>A home address as the app sends it (sign-up and personal details). Same as the API's.</summary>
public sealed record HomeAddressInput(string Line1, string? Line2, string City, string RegionOrPostcode,
                                      Country Country);
