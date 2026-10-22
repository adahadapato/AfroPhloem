using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One of my shop's documents. Same shape as the API's <c>MyShopDocumentDto</c>.</summary>
public sealed record MyShopDocumentDto(
    Guid Id,
    VendorDocumentType Type,
    DocumentStatus Status,
    string OriginalFileName,
    string? RejectionReason,
    DateTime? ExpiresAt,
    bool IsExpired,
    DateTime UploadedAt);
