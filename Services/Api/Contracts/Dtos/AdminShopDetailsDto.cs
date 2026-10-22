using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One shop in full for an admin: pictures, documents and what's missing for the ✓ badge.
/// Same shape as the API's <c>AdminShopDetailsDto</c>.</summary>
public sealed record AdminShopDetailsDto(
    AdminShopDto Shop,
    string? LogoUrl,
    string? CoverPhotoUrl,
    IReadOnlyList<AdminShopDocumentDto> Documents,
    IReadOnlyList<VendorDocumentType> MissingDocuments);
