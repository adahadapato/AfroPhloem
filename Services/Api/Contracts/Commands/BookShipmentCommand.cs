using Phloem.Services.Api.Contracts.Dtos;
using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of <c>POST /api/shipments</c>. Same shape as the API's <c>BookShipmentCommand</c>.</summary>
public sealed record BookShipmentCommand(
    Country FromCountry,
    Country ToCountry,
    string SenderPhone,
    string RecipientName,
    string RecipientPhone,
    string RecipientAddressLine,
    string RecipientCity,
    string RecipientRegionOrPostcode,
    IReadOnlyList<ShipmentItemInputDto> Items,
    DeliverySpeed Speed,
    decimal EstimatedWeightKg,
    bool ConfirmedNoProhibitedItems,
    bool AcceptedTerms);
