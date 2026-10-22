namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of "new price" (<c>PUT /api/shops/mine/dishes/{dishId}/price</c>). Same shape as the API's
/// <c>DishEndpoints.PriceRequest</c>.</summary>
public sealed record PriceRequest(decimal Price);
