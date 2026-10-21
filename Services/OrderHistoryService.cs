using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using Phloem.Models;
using Phloem.Services.Api.Contracts.Dtos;
using Phloem.Services.Api.Contracts.Enums;
using Phloem.Settings;

namespace Phloem.Services;

/// <summary>One dish + quantity inside a placed order (used by "Buy again" and Order details).</summary>
public record OrderLine(Dish Dish, int Quantity)
{
    /// <summary>Price at the time of ordering (so later price changes don't alter old orders).</summary>
    public decimal UnitPrice { get; init; } = Dish.Price;

    /// <summary>The order's currency symbol, e.g. "GH₵".</summary>
    public string CurrencySymbol { get; init; } = Dish.CurrencySymbol;

    public decimal LineTotal => UnitPrice * Quantity;
    public string LineTotalText => $"{CurrencySymbol}{LineTotal:N2}";
    public string QuantityText => $"×{Quantity}";
}

/// <summary>A placed order, shown under "Recent orders" on the Orders page and on Order details.</summary>
/// <remarks>
/// Orders come from the API (<see cref="OrderHistoryService.LoadAsync"/>). "Send to UK" orders are still made on the
/// phone (<see cref="IsInternational"/>) until the International batch connects them to the API.
/// </remarks>
public partial class PlacedOrder : ObservableObject
{
    /// <summary>How long after paying the customer can still cancel (the API's rule; used for Send-to-UK orders).</summary>
    public static readonly TimeSpan CancelWindow = TimeSpan.FromMinutes(30);

    public required string OrderNumber { get; init; }
    public DateTime PlacedAt { get; init; } = DateTime.Now;

    /// <summary>The shop the order is from.</summary>
    public string ShopName { get; init; } = string.Empty;

    /// <summary>A picture of one of the dishes (for the list), or null.</summary>
    public string? ImageUrl { get; init; }

    /// <summary>e.g. "Jollof Rice ×2, Egusi Soup ×1"</summary>
    public string ItemsSummary { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public decimal Subtotal { get; init; }
    public decimal DeliveryFee { get; init; }
    public decimal Total { get; init; }

    /// <summary>"Home · 31 Camden High St, London" (or the UK recipient for Send-to-UK orders).</summary>
    public string DeliveryAddress { get; init; } = string.Empty;
    public string CurrencySymbol { get; init; } = "£";
    public string PaymentMethod { get; init; } = string.Empty;

    /// <summary>The dishes and quantities (filled on Order details; empty in the list).</summary>
    public IReadOnlyList<OrderLine> Lines { get; init; } = Array.Empty<OrderLine>();

    /// <summary>The "Send to UK" shipment that came with this order, if any.</summary>
    public DeliveryOrder? Shipment { get; init; }
    public bool IsInternational => Shipment is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCancelled), nameof(CanCancel), nameof(StatusText), nameof(IsPreparing))]
    public partial OrderStatus Status { get; set; } = OrderStatus.Paid;

    /// <summary>The API's answer to "can it still be cancelled?" when the order was loaded.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCancel), nameof(StatusText))]
    public partial bool AllowsCancel { get; set; } = true;

    /// <summary>When cancelling stops being possible; null = no time limit (e.g. not paid yet).</summary>
    public DateTime? CancelDeadline { get; init; }

    public bool IsCancelled => Status == OrderStatus.Cancelled;

    /// <summary>True while the order can still be cancelled (the API has the final say).</summary>
    public bool CanCancel => !IsCancelled && AllowsCancel && (CancelDeadline is null || DateTime.Now < CancelDeadline);

    public string StatusText => Status switch
    {
        OrderStatus.AwaitingPayment => "Awaiting payment",
        OrderStatus.Paid => IsInternational ? "Shipping to UK" : "Order placed",
        OrderStatus.Preparing => "Being prepared",
        OrderStatus.OutForDelivery => IsInternational ? "Shipping to UK" : "On its way",
        OrderStatus.Delivered => "Delivered",
        OrderStatus.Cancelled => "Cancelled",
        _ => Status.ToString()
    };

    public string TotalText => $"{CurrencySymbol}{Total:N2}";
    public string SubtotalText => $"{CurrencySymbol}{Subtotal:N2}";
    public string DeliveryFeeText => $"{CurrencySymbol}{DeliveryFee:N2}";

    /// <summary>When the order was cancelled (shown on Order details).</summary>
    [ObservableProperty]
    public partial DateTime? CancelledAt { get; set; }

    // ---------- Progress tracker (Order details) ----------
    /// <summary>Step 2 done once the shop has started.</summary>
    public bool IsPreparing => Status is OrderStatus.Preparing or OrderStatus.OutForDelivery or OrderStatus.Delivered;
    public string PlacedAtText => PlacedAt.ToString("d MMM yyyy, HH:mm");
    public string ThirdStepTitle => IsInternational ? "Shipping to the UK" : "On its way";

    /// <summary>Re-check the time-based cancel window (called when the Orders page refreshes).</summary>
    public void RefreshTiming()
    {
        OnPropertyChanged(nameof(CanCancel));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(IsPreparing));
    }
}

/// <summary>
/// "My orders": the signed-in user's orders from the API (<see cref="Orders"/>), plus Cancel and Buy again.
/// Emptied when the user signs out and reloaded when they sign in. Register as a singleton.
/// </summary>
public class OrderHistoryService
{
    private readonly OrderService _orders;
    private readonly SessionService _session;
    private readonly ApiSettings _api;
    private Task? _loading;

    public ObservableCollection<PlacedOrder> Orders { get; } = new();

    public OrderHistoryService(OrderService orders, SessionService session, IOptions<ApiSettings> api)
    {
        _orders = orders;
        _session = session;
        _api = api.Value;

        // Signed out → nobody else on this phone sees the orders; signed in → load theirs
        session.Changed += (_, _) => MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!session.IsLoggedIn)
            {
                Orders.Clear();
                return;
            }

            try { await LoadAsync(); }
            catch (Exception) { /* the Orders page loads them again when it opens */ }
        });
    }

    /// <summary>Adds an order made on the phone ("Send to UK", until it's connected to the API).</summary>
    public void Add(PlacedOrder order) => Orders.Insert(0, order);

    // ───────────────────────────── 1. Loading ─────────────────────────────

    /// <summary>Reloads "My orders" from the API (several calls at once share one load).</summary>
    /// <returns>Null when loaded (or signed out); otherwise why it failed.</returns>
    public async Task<string?> LoadAsync()
    {
        if (!_session.IsLoggedIn)
        {
            Orders.Clear();
            return null;
        }

        string? error = null;
        _loading ??= LoadCoreAsync(e => error = e);
        try { await _loading; }
        finally { _loading = null; }
        return error;
    }

    private async Task LoadCoreAsync(Action<string> fail)
    {
        var result = await _orders.GetMyOrdersAsync();
        if (result.IsFailure)
        {
            fail(result.Error!.DisplayMessage);
            return;
        }

        // Keep "Send to UK" orders made on the phone; the rest come from the API
        var local = Orders.Where(o => o.IsInternational).ToList();
        Orders.Clear();
        foreach (var order in local.Concat(result.Value!.Items.Select(ToPlacedOrder))
                                   .OrderByDescending(o => o.PlacedAt))
            Orders.Add(order);
    }

    /// <summary>The full order (with its dishes) for Order details; null with a message when it can't be loaded.</summary>
    public async Task<(PlacedOrder? Order, string? Error)> LoadDetailsAsync(string orderNumber)
    {
        var result = await _orders.GetAsync(orderNumber);
        return result.IsFailure ? (null, result.Error!.DisplayMessage) : (ToPlacedOrder(result.Value!), null);
    }

    // ───────────────────────────── 2. Cancel ─────────────────────────────

    /// <summary>
    /// Cancel order (shared by the Orders list and Order details): asks first, then cancels on the API.
    /// "Send to UK" orders are still cancelled on the phone (and their shipment withdrawn).
    /// </summary>
    public async Task<bool> CancelAsync(PlacedOrder order, MockDataService data)
    {
        if (!order.CanCancel)
        {
            await Shell.Current.DisplayAlertAsync("Can't cancel",
                "This order is already being prepared, so it can no longer be cancelled.", "OK");
            order.RefreshTiming();
            return false;
        }

        var refund = order.Status == OrderStatus.AwaitingPayment
            ? "Nothing has been paid for it yet."
            : "Your payment will be refunded.";
        var confirm = await Shell.Current.DisplayAlertAsync("Cancel this order?",
            $"{order.OrderNumber} · {order.TotalText}\n{refund}", "Cancel order", "Keep order");
        if (!confirm) return false;

        if (!order.IsInternational)
        {
            var result = await _orders.CancelAsync(order.OrderNumber);
            if (result.IsFailure)
            {
                await Shell.Current.DisplayAlertAsync("Can't cancel", result.Error!.DisplayMessage, "OK");
                await LoadAsync();   // show what the order's status really is now
                return false;
            }
        }
        else if (order.Shipment is not null)
        {
            data.Shipments.Remove(order.Shipment);
        }

        order.Status = OrderStatus.Cancelled;
        order.AllowsCancel = false;
        order.CancelledAt = DateTime.Now;
        return true;
    }

    // ───────────────────────────── 3. Buy again ─────────────────────────────

    /// <summary>Buy again: put the same dishes and quantities back in the basket, then offer Checkout.</summary>
    public async Task BuyAgainAsync(PlacedOrder order, CartService cart)
    {
        IReadOnlyList<OrderLine>? lines = order.Lines;
        IReadOnlyList<string> unavailable = [];
        if (!order.IsInternational)
            (lines, unavailable) = await LinesToBuyAgainAsync(order.OrderNumber);

        if (lines is null) return;   // the error was already shown

        foreach (var line in lines)
            for (var i = 0; i < line.Quantity; i++)
                cart.AddDish(line.Dish);

        var count = lines.Sum(l => l.Quantity);
        var message = count == 0
            ? "None of these dishes are on sale right now."
            : $"{count} item(s) from {order.OrderNumber} are back in your basket.";
        if (unavailable.Count > 0)
            message += $"\nNo longer available: {string.Join(", ", unavailable)}.";

        if (count == 0)
        {
            await Shell.Current.DisplayAlertAsync("Buy again", message, "OK");
            return;
        }

        var go = await Shell.Current.DisplayAlertAsync("Added to your basket", message, "Checkout", "Keep shopping");
        if (go)
            await AppNav.GoToCheckoutAsync();
    }

    /// <summary>The dishes still on sale (from the API), with their names and pictures from the order.</summary>
    private async Task<(IReadOnlyList<OrderLine>? Lines, IReadOnlyList<string> Unavailable)> LinesToBuyAgainAsync(
        string orderNumber)
    {
        var again = await _orders.BuyAgainAsync(orderNumber);
        var details = again.IsSuccess ? await _orders.GetAsync(orderNumber) : null;
        var error = again.IsFailure ? again.Error : details?.IsFailure == true ? details.Error : null;
        if (error is not null)
        {
            await Shell.Current.DisplayAlertAsync("Buy again", error.DisplayMessage, "OK");
            return (null, []);
        }

        var order = details!.Value!;
        var symbol = OrderService.SymbolOf(order.Currency);
        var lines = again.Value!.Items
            .Select(item => (Item: item, Line: order.Lines.FirstOrDefault(l => l.DishId == item.DishId)))
            .Where(x => x.Line is not null)
            .Select(x => new OrderLine(ToDish(x.Line!, order.ShopName, symbol), x.Item.Quantity))
            .ToList();
        return (lines, again.Value.UnavailableDishes);
    }

    // ───────────────────────────── 4. API → app models ─────────────────────────────

    private PlacedOrder ToPlacedOrder(OrderSummaryDto o) => new()
    {
        OrderNumber = o.OrderNumber,
        ShopName = o.ShopName,
        ImageUrl = _api.ResolveFileUrl(o.ImageUrl),
        PlacedAt = o.PlacedAt.ToLocalTime(),
        ItemsSummary = o.ItemsSummary,
        ItemCount = o.ItemCount,
        Total = o.Total,
        CurrencySymbol = OrderService.SymbolOf(o.Currency),
        PaymentMethod = PaymentText(o.Status),
        Status = o.Status,
        AllowsCancel = o.CanCancel,
        CancelDeadline = o.Status == OrderStatus.Paid && o.PaidAt is { } paid ? paid.ToLocalTime() + PlacedOrder.CancelWindow : null
    };

    private PlacedOrder ToPlacedOrder(OrderDetailsDto o)
    {
        var symbol = OrderService.SymbolOf(o.Currency);
        return new PlacedOrder
        {
            OrderNumber = o.OrderNumber,
            ShopName = o.ShopName,
            ImageUrl = _api.ResolveFileUrl(o.Lines.FirstOrDefault(l => l.ImageUrl is not null)?.ImageUrl),
            PlacedAt = o.PlacedAt.ToLocalTime(),
            ItemsSummary = string.Join(", ", o.Lines.Select(l => $"{l.DishName} ×{l.Quantity}")),
            ItemCount = o.Lines.Sum(l => l.Quantity),
            Subtotal = o.Subtotal,
            DeliveryFee = o.DeliveryFee,
            Total = o.Total,
            DeliveryAddress = string.IsNullOrWhiteSpace(o.DeliveryLabel)
                ? o.DeliveryAddress
                : $"{o.DeliveryLabel} · {o.DeliveryAddress}",
            CurrencySymbol = symbol,
            PaymentMethod = PaymentText(o.Status),
            Lines = o.Lines.Select(l => new OrderLine(ToDish(l, o.ShopName, symbol), l.Quantity)).ToList(),
            Status = o.Status,
            AllowsCancel = o.CanCancel,
            CancelDeadline = o.CancelSecondsLeft is { } seconds ? DateTime.Now.AddSeconds(seconds) : null,
            CancelledAt = o.CancelledAt?.ToLocalTime()
        };
    }

    private Dish ToDish(OrderLineDto line, string shopName, string symbol) => new()
    {
        Id = line.DishId.ToString(),
        Name = line.DishName,
        ImageUrl = _api.ResolveFileUrl(line.ImageUrl) ?? string.Empty,
        Price = line.UnitPrice,
        VendorName = shopName,
        CurrencySymbol = symbol,
        IsAvailable = !line.IsOutOfStock
    };

    /// <summary>What the order list says about payment (only test payments exist for now).</summary>
    private static string PaymentText(OrderStatus status) => status switch
    {
        OrderStatus.AwaitingPayment => "Not paid yet",
        OrderStatus.Cancelled => "Cancelled",
        _ => "Test payment"
    };
}
