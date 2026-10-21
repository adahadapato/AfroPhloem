using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One item the sender declares in a parcel. Same shape as the API's <c>ShipmentItemInputDto</c>.</summary>
public sealed record ShipmentItemInputDto(
    ShipmentItemCategory Category,
    string Description,
    int Quantity,
    decimal UnitDeclaredValue);
