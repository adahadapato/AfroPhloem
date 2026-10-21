using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>
/// One shop's part of a checked basket (each shop becomes its own order). Same shape as the API's
/// <c>BasketShopDto</c>. <see cref="DeliveryProblem"/> says why it can't be delivered, when it can't.
/// </summary>
public sealed record BasketShopDto(
    Guid VendorId,
    string ShopName,
    Currency Currency,
    IReadOnlyList<BasketLineDto> Lines,
    decimal Subtotal,
    decimal? DeliveryFee,
    string? DeliveryProblem,
    decimal? Total);
