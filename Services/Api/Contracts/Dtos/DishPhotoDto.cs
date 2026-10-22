namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One photo of a dish. Same shape as the API's <c>DishPhotoDto</c>.</summary>
public sealed record DishPhotoDto(
    Guid Id,
    string Url,
    string? ThumbnailUrl,
    string AltText,
    int DisplayOrder,
    bool IsPrimary);
