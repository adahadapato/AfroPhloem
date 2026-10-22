using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>
/// One shop on the admin dashboard (with its logo). <see cref="IsOwnerSuspended"/>: hidden because its owner is
/// suspended (its own status is unchanged). Same shape as the API's <c>AdminShopDto</c>.
/// </summary>
public sealed record AdminShopDto(
    Guid Id,
    string Name,
    string OwnerName,
    string BusinessEmail,
    string? BusinessPhone,
    Country Country,
    VendorStatus Status,
    string? RejectionReason,
    bool IsVerified,
    decimal Rating,
    int RatingCount,
    DateTime OpenedAt,
    string? LogoUrl,
    bool IsOwnerSuspended);
