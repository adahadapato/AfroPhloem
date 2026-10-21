using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One shop card (Top sellers, Sellers &amp; Vendors). Same shape as the API's <c>ShopListItemDto</c>.</summary>
public sealed record ShopListItemDto(
    Guid Id,
    string Name,
    Country Country,
    bool IsVerified,
    decimal Rating,
    int RatingCount,
    int DishCount,
    string? LogoUrl,
    string? CoverPhotoUrl);
