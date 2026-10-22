using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>"Can't find your dish?" (<c>POST /api/dish-catalog/suggestions</c>): waits for an admin. Same shape as the
/// API's <c>SuggestDishNameCommand</c>.</summary>
public sealed record SuggestDishNameCommand(string Name, DishCategory Category, CatalogGroup Group);
