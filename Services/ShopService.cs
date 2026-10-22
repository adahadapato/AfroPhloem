using Microsoft.Extensions.Options;
using Phloem.Models;
using Phloem.Services.Api;
using Phloem.Services.Api.Contracts;
using Phloem.Services.Api.Contracts.Dtos;
using Phloem.Services.Api.Contracts.Enums;
using Phloem.Settings;

namespace Phloem.Services;

/// <summary>
/// Shops for buyers: <c>/api/shops</c>. Returns the app's own <see cref="Vendor"/> model, so the existing screens bind
/// to it unchanged (name, logo, cover photo, rating, ✓, dish count). Only approved shops are ever returned.
/// </summary>
public sealed class ShopService(ApiClient api, IOptions<ApiSettings> settings)
{
    /// <summary>"Meet local shops": approved shops, optionally in one country and/or matching a name; one page.</summary>
    /// <param name="country">Only this country; null = every country.</param>
    /// <param name="search">Part of a shop's name; empty = no filter.</param>
    /// <param name="page">1-based page.</param>
    /// <param name="pageSize">Shops per page.</param>
    /// <param name="ct">Cancels the request.</param>
    public async Task<ApiResult<PagedResult<Vendor>>> GetShopsAsync(Country? country, string? search, int page,
                                                                   int pageSize, CancellationToken ct = default)
    {
        var path = ApiQuery.Build("api/shops", new Dictionary<string, string?>
        {
            ["country"] = country?.ToString(),
            ["search"] = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString()
        });

        var result = await api.GetAsync<PagedResult<ShopListItemDto>>(path, ct);
        if (result.IsFailure) return result.Error!;

        var paged = result.Value!;
        return ApiResult<PagedResult<Vendor>>.Success(
            new PagedResult<Vendor>(paged.Items.Select(ToVendor).ToList(), paged.TotalCount, paged.Page, paged.PageSize));
    }

    /// <summary>Home → "Top sellers": the best-rated shops.</summary>
    /// <param name="country">Only this country; null = every country.</param>
    /// <param name="count">How many (1–20).</param>
    /// <param name="ct">Cancels the request.</param>
    public async Task<ApiResult<IReadOnlyList<Vendor>>> GetTopShopsAsync(Country? country, int count = 8,
                                                                        CancellationToken ct = default)
    {
        var path = ApiQuery.Build("api/shops/top", new Dictionary<string, string?>
        {
            ["country"] = country?.ToString(),
            ["count"] = count.ToString()
        });

        var result = await api.GetAsync<List<ShopListItemDto>>(path, ct);
        return result.IsFailure
            ? result.Error!
            : ApiResult<IReadOnlyList<Vendor>>.Success(result.Value!.Select(ToVendor).ToList());
    }

    /// <summary>The API's shop card as the app's <see cref="Vendor"/>.</summary>
    private Vendor ToVendor(ShopListItemDto s) => new()
    {
        Id = s.Id.ToString(),
        Name = s.Name,
        LogoUrl = settings.Value.ResolveFileUrl(s.LogoUrl) ?? Vendor.DefaultLogo,
        CoverPhotoUrl = settings.Value.ResolveFileUrl(s.CoverPhotoUrl) ?? Vendor.DefaultCoverPhoto,
        Rating = (double)s.Rating,
        RatingCount = s.RatingCount,
        Country = CountryOption.NameOf(s.Country),
        DishCount = s.DishCount,
        IsVerified = s.IsVerified,
        IsApproved = true
    };
}
