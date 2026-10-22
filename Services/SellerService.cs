using Phloem.Services.Api;
using Phloem.Services.Api.Contracts;
using Phloem.Services.Api.Contracts.Commands;
using Phloem.Services.Api.Contracts.Dtos;
using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services;

/// <summary>
/// The signed-in seller's shop and its orders on the API: <c>/api/shops/mine</c> and <c>/api/shops/mine/orders</c>.
/// "mine" comes from the sign-in, never from the app. Screens: Seller dashboard and the seller's Order page.
/// </summary>
public sealed class SellerService(ApiClient api)
{
    // ───────────────────────────── 1. My shop ─────────────────────────────

    /// <summary>My shop (404 when the user hasn't opened one).</summary>
    public Task<ApiResult<MyShopDto>> GetMyShopAsync(CancellationToken ct = default) =>
        api.GetAsync<MyShopDto>("api/shops/mine", ct);

    /// <summary>Orders per status and this week's daily sales.</summary>
    public Task<ApiResult<ShopOrderStatsDto>> GetStatsAsync(CancellationToken ct = default) =>
        api.GetAsync<ShopOrderStatsDto>("api/shops/mine/orders/stats", ct);

    // ───────────────────────────── 2. Orders ─────────────────────────────

    /// <summary>My shop's orders with one status, newest first (Paid = waiting to be accepted); one page.</summary>
    public Task<ApiResult<PagedResult<ShopOrderSummaryDto>>> GetOrdersAsync(OrderStatus status, int page, int pageSize,
                                                                            CancellationToken ct = default) =>
        api.GetAsync<PagedResult<ShopOrderSummaryDto>>(
            $"api/shops/mine/orders?status={status}&page={page}&pageSize={pageSize}", ct);

    /// <summary>One order with its dishes: what the customer ordered, before accepting it.</summary>
    public Task<ApiResult<OrderDetailsDto>> GetOrderAsync(string orderNumber, CancellationToken ct = default) =>
        api.GetAsync<OrderDetailsDto>($"api/orders/{Uri.EscapeDataString(orderNumber)}", ct);

    /// <summary>Accept a waiting order and start preparing it.</summary>
    public Task<ApiResult> AcceptAsync(string orderNumber, CancellationToken ct = default) =>
        api.PostAsync($"{OrderPath(orderNumber)}/accept", null, ct);

    /// <summary>Decline a waiting order: it's cancelled and refunded, and the customer is emailed the reason.</summary>
    public Task<ApiResult> DeclineAsync(string orderNumber, string reason, CancellationToken ct = default) =>
        api.PostAsync($"{OrderPath(orderNumber)}/decline", new DeclineRequest(reason), ct);

    /// <summary>The order has left the kitchen.</summary>
    public Task<ApiResult> SendOutAsync(string orderNumber, CancellationToken ct = default) =>
        api.PostAsync($"{OrderPath(orderNumber)}/send-out", null, ct);

    /// <summary>The order was delivered (the earnings go to my balance).</summary>
    public Task<ApiResult> MarkDeliveredAsync(string orderNumber, CancellationToken ct = default) =>
        api.PostAsync($"{OrderPath(orderNumber)}/delivered", null, ct);

    private static string OrderPath(string orderNumber) =>
        $"api/shops/mine/orders/{Uri.EscapeDataString(orderNumber)}";
}
