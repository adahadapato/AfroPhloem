namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>Seller dashboard numbers (<c>GET /api/shops/mine/orders/stats</c>). Same shape as the API's
/// <c>ShopOrderStatsDto</c>. <see cref="NewOrders"/> are paid orders waiting to be accepted.</summary>
public sealed record ShopOrderStatsDto(
    int NewOrders,
    int Preparing,
    int OutForDelivery,
    int Delivered,
    int TotalOrders,
    IReadOnlyList<DailySalesDto> WeeklySales);
