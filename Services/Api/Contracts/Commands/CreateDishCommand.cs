namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Add a dish (<c>POST /api/shops/mine/dishes</c>): a catalog name + optional detail, a price inside the
/// name's range. Same shape as the API's <c>CreateDishCommand</c>.</summary>
public sealed record CreateDishCommand(Guid DishCatalogId, string? Detail, decimal Price, string? Description,
                                       bool IsAvailable);
