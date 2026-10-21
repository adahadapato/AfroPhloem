using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One speed's price on a route (admin). Same shape as the API's <c>ShipmentRateDto</c>.</summary>
public sealed record ShipmentRateDto(
    DeliverySpeed Speed,
    decimal BaseCharge,
    decimal PricePerKg,
    int MinTransitDays,
    int MaxTransitDays);
