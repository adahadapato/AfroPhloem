using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One row of "My orders" (<c>GET /api/orders</c>). Same shape as the API's <c>OrderSummaryDto</c>.</summary>
public sealed record OrderSummaryDto(
    string OrderNumber,
    string ShopName,
    OrderStatus Status,
    DateTime PlacedAt,
    DateTime? PaidAt,
    int ItemCount,
    string ItemsSummary,
    string? ImageUrl,
    decimal Total,
    Currency Currency,
    bool HasPriceChanges,
    int OutOfStockCount,
    bool CanCancel);
