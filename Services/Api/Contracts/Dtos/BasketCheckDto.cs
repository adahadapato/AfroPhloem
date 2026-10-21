namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>Answer of <c>POST /api/orders/basket/check</c>. Same shape as the API's <c>BasketCheckDto</c>.</summary>
public sealed record BasketCheckDto(
    IReadOnlyList<BasketShopDto> Shops,
    IReadOnlyList<Guid> MissingDishIds,
    bool CanPlaceOrder);
