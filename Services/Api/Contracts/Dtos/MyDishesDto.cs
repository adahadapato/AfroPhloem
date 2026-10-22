namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>My dishes (<c>GET /api/shops/mine/dishes</c>), those needing a new price first. Same shape as the API's
/// <c>MyDishesDto</c>.</summary>
public sealed record MyDishesDto(IReadOnlyList<MyDishDto> Dishes, int NeedsPriceUpdateCount);
