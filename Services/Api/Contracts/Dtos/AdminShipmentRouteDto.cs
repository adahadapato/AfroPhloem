using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>A route as the admin sees it (on or off, with every rate). Same shape as the API's <c>AdminShipmentRouteDto</c>.</summary>
public sealed record AdminShipmentRouteDto(
    Country FromCountry,
    Country ToCountry,
    Currency Currency,
    string DropOffAddress,
    string DropOffHours,
    bool IsActive,
    IReadOnlyList<ShipmentRateDto> Rates);
