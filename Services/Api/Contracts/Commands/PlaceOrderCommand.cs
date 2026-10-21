using Phloem.Services.Api.Contracts.Dtos;
using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>
/// Body of <c>POST /api/orders</c>. Same shape as the API's <c>PlaceOrderCommand</c>.
/// <see cref="DeliveryAddressId"/> is one of the user's saved addresses (<see cref="AddressDto.Id"/>).
/// </summary>
public sealed record PlaceOrderCommand(
    IReadOnlyList<BasketItemDto> Items,
    Guid DeliveryAddressId,
    DeliveryWindow DeliveryWindow);
