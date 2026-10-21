namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One dish in an order. Same shape as the API's <c>OrderLineDto</c>.</summary>
public sealed record OrderLineDto(
    Guid DishId,
    string DishName,
    string? ImageUrl,
    int Quantity,
    decimal UnitPrice,
    decimal? PriceWhenAdded,
    bool PriceChanged,
    bool IsOutOfStock,
    decimal LineTotal);
