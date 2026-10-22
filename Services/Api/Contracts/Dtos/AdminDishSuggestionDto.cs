using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>A dish name a seller suggested, waiting for review. Same shape as the API's <c>AdminDishSuggestionDto</c>.</summary>
public sealed record AdminDishSuggestionDto(
    Guid Id,
    string Name,
    DishCategory Category,
    CatalogGroup Group,
    string SuggestedBy,
    string? ShopName,
    DateTime SuggestedAt);
