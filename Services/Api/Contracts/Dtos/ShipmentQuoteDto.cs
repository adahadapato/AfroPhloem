using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>Estimated price and transit time. Same shape as the API's <c>ShipmentQuoteDto</c>.</summary>
public sealed record ShipmentQuoteDto(
    DeliverySpeed Speed,
    decimal EstimatedWeightKg,
    decimal ChargeableWeightKg,
    decimal EstimatedPrice,
    Currency Currency,
    int MinTransitDays,
    int MaxTransitDays);
