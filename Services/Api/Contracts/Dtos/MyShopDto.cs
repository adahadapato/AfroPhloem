using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>
/// The signed-in seller's own shop (<c>GET /api/shops/mine</c>). Same shape as the API's <c>MyShopDto</c>.
/// <see cref="OrderCutoffTime"/>: after it (the shop's local time) buyers can only choose Tomorrow.
/// </summary>
public sealed record MyShopDto(
    Guid Id,
    string Name,
    string BusinessEmail,
    string? BusinessPhone,
    Country Country,
    Currency Currency,
    VendorStatus Status,
    string? RejectionReason,
    bool IsLive,
    bool IsVerified,
    string? LogoUrl,
    string? CoverPhotoUrl,
    decimal? DeliveryFee,
    decimal Rating,
    int RatingCount,
    decimal Balance,
    DateTime OpenedAt,
    IReadOnlyList<MyShopDocumentDto> Documents,
    IReadOnlyList<VendorDocumentType> MissingDocuments,
    TimeOnly OrderCutoffTime);
