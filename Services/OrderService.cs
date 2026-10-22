using Phloem.Models;
using Phloem.Services.Api;
using Phloem.Services.Api.Contracts;
using Phloem.Services.Api.Contracts.Dtos;
using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services;

/// <summary>
/// The buyer's orders on the API (<c>/api/orders</c>): check the basket, place, pay (test payment for now), list,
/// open, cancel and buy again. Screens use <see cref="OrderHistoryService"/> and <c>CheckoutViewModel</c>; this class
/// only talks to the API.
/// </summary>
public sealed class OrderService(ApiClient api)
{
    // ───────────────────────────── 1. Basket ─────────────────────────────

    /// <summary>The basket's dishes as the API wants them (Id + quantity). Dishes without an API Id are skipped.</summary>
    /// <param name="items">The basket.</param>
    public static IReadOnlyList<BasketItemDto> ToBasketItems(IEnumerable<CartItem> items) =>
        items.Where(i => i.Quantity > 0 && Guid.TryParse(i.Dish.Id, out _))
             .GroupBy(i => Guid.Parse(i.Dish.Id))
             .Select(g => new BasketItemDto(g.Key, g.Sum(i => i.Quantity)))
             .ToList();

    /// <summary>Today's prices, delivery fees and problems for the basket (works signed out too; nothing is saved).</summary>
    public Task<ApiResult<BasketCheckDto>> CheckBasketAsync(IReadOnlyList<BasketItemDto> items, DeliveryWindow window,
                                                           CancellationToken ct = default) =>
        api.PostAsync<BasketCheckDto>("api/orders/basket/check", new CheckBasketQuery(items, window), ct);

    // ───────────────────────────── 2. Place and pay ─────────────────────────────

    /// <summary>Places the basket: one unpaid order per shop.</summary>
    public Task<ApiResult<List<PlacedOrderDto>>> PlaceAsync(IReadOnlyList<BasketItemDto> items, Guid deliveryAddressId,
                                                            DeliveryWindow window, CancellationToken ct = default) =>
        api.PostAsync<List<PlacedOrderDto>>("api/orders", new PlaceOrderCommand(items, deliveryAddressId, window), ct);

    /// <summary>
    /// TEST ONLY: pays an order without money moving, so the whole flow can be tried before real payments
    /// (Paystack / Stripe) are connected. The API answers 404 once test payments are switched off.
    /// </summary>
    public Task<ApiResult<OrderPaymentDto>> PayWithTestPaymentAsync(string orderNumber, CancellationToken ct = default) =>
        api.PostAsync<OrderPaymentDto>($"api/orders/{Uri.EscapeDataString(orderNumber)}/test-payment", null, ct);

    // ───────────────────────────── 3. My orders ─────────────────────────────

    /// <summary>My orders, newest first; one page.</summary>
    public Task<ApiResult<PagedResult<OrderSummaryDto>>> GetMyOrdersAsync(int page, int pageSize,
                                                                         CancellationToken ct = default) =>
        api.GetAsync<PagedResult<OrderSummaryDto>>($"api/orders?page={page}&pageSize={pageSize}", ct);

    /// <summary>One of my orders, with its dishes.</summary>
    public Task<ApiResult<OrderDetailsDto>> GetAsync(string orderNumber, CancellationToken ct = default) =>
        api.GetAsync<OrderDetailsDto>($"api/orders/{Uri.EscapeDataString(orderNumber)}", ct);

    /// <summary>Cancels one of my orders (the API decides whether it still can be).</summary>
    public Task<ApiResult> CancelAsync(string orderNumber, CancellationToken ct = default) =>
        api.PostAsync($"api/orders/{Uri.EscapeDataString(orderNumber)}/cancel", null, ct);

    /// <summary>The order's dishes that are still on sale, ready to go back in the basket.</summary>
    public Task<ApiResult<BuyAgainDto>> BuyAgainAsync(string orderNumber, CancellationToken ct = default) =>
        api.GetAsync<BuyAgainDto>($"api/orders/{Uri.EscapeDataString(orderNumber)}/buy-again", ct);

    // ───────────────────────────── 4. Display ─────────────────────────────

    /// <summary>The symbol for a currency, e.g. GHS → "GH₵" (from the countries loaded at start-up).</summary>
    public static string SymbolOf(Currency currency) =>
        CountryOption.All.FirstOrDefault(c => c.Currency == currency)?.CurrencySymbol ?? $"{currency} ";
}
