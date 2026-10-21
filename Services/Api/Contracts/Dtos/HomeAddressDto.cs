using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>The signed-in user's home address, as the API returns it.</summary>
public sealed record HomeAddressDto(string Line1, string? Line2, string City, string RegionOrPostcode,
                                    Country Country, string SingleLine);
