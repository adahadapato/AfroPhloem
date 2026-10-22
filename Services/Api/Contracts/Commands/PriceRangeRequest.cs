namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of "set a price range" (<c>PUT /api/admin/dish-catalog/{id}/price-ranges/{country}</c>); both ends
/// allowed. Same shape as the API's <c>DishCatalogEndpoints.PriceRangeRequest</c>.</summary>
public sealed record PriceRangeRequest(decimal MinPrice, decimal MaxPrice);
