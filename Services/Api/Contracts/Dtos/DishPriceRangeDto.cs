using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>A dish name's allowed price range (inclusive) in one country. Same shape as the API's
/// <c>DishPriceRangeDto</c>.</summary>
public sealed record DishPriceRangeDto(
    Guid Id,
    Guid DishCatalogId,
    Country Country,
    Currency Currency,
    decimal MinPrice,
    decimal MaxPrice);
