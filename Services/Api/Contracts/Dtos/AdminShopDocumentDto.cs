using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One document a shop uploaded. Same shape as the API's <c>AdminShopDocumentDto</c>.</summary>
public sealed record AdminShopDocumentDto(
    Guid Id,
    VendorDocumentType Type,
    DocumentStatus Status,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string? RejectionReason,
    DateTime? ExpiresAt,
    bool IsExpired,
    DateTime UploadedAt,
    DateTime? ReviewedAt);
