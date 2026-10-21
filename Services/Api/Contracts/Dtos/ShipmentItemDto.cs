using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One item in a parcel. Same shape as the API's <c>ShipmentItemDto</c>.</summary>
public sealed record ShipmentItemDto(
    ShipmentItemCategory Category,
    string Description,
    int Quantity,
    decimal UnitDeclaredValue,
    decimal TotalDeclaredValue);
