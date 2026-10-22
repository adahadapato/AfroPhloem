namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of "change a dish" (<c>PUT /api/shops/mine/dishes/{dishId}</c>). Same shape as the API's
/// <c>DishEndpoints.DishRequest</c>.</summary>
public sealed record DishRequest(Guid DishCatalogId, string? Detail, decimal Price, string? Description,
                                 bool IsAvailable);
