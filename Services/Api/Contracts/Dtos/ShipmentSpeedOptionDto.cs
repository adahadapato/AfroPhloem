using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One speed offered on a route, with its price. Same shape as the API's <c>ShipmentSpeedOptionDto</c>.</summary>
public sealed record ShipmentSpeedOptionDto(
    DeliverySpeed Speed,
    decimal BaseCharge,
    decimal PricePerKg,
    int MinTransitDays,
    int MaxTransitDays);
