using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Phloem.Models;

namespace Phloem.Services;

public enum OrderStatus { Placed, Cancelled }

/// <summary>One dish + quantity inside a placed order (used by "Buy again" and Order details).</summary>
public record OrderLine(Dish Dish, int Quantity)
{
    /// <summary>Price at the time of ordering (so later price changes don't alter old orders).</summary>
    public decimal UnitPrice { get; init; } = Dish.Price;
    public decimal LineTotal => UnitPrice * Quantity;
    public string LineTotalText => $"£{LineTotal:0.00}";
    public string QuantityText => $"×{Quantity}";
}

/// <summary>A placed order, shown under "Recent orders" on the Orders page.</summary>
public partial class PlacedOrder : ObservableObject
{
    /// <summary>How long after ordering the customer can still cancel.</summary>
    public static readonly TimeSpan CancelWindow = TimeSpan.FromMinutes(30);

    public required string OrderNumber { get; init; }
    public DateTime PlacedAt { get; init; } = DateTime.Now;

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

    /// <summary>The dishes and quantities, so "Buy again" can refill the basket.</summary>
    public IReadOnlyList<OrderLine> Lines { get; init; } = Array.Empty<OrderLine>();

    /// <summary>The "Send to UK" shipment that came with this order, if any.</summary>
    public DeliveryOrder? Shipment { get; init; }
    public bool IsInternational => Shipment is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCancelled), nameof(CanCancel), nameof(StatusText), nameof(IsPreparing))]
    public partial OrderStatus Status { get; set; } = OrderStatus.Placed;

    public bool IsCancelled => Status == OrderStatus.Cancelled;

    /// <summary>True while the order is still within the cancel window.</summary>
    public bool CanCancel => Status == OrderStatus.Placed && DateTime.Now - PlacedAt < CancelWindow;

    public string StatusText => Status switch
    {
        OrderStatus.Cancelled => "Cancelled",
        _ when CanCancel => IsInternational ? "Shipping to UK" : "Order placed",
        _ => IsInternational ? "Shipping to UK" : "Being prepared"
    };

    public string TotalText => $"{CurrencySymbol}{Total:0.00}";
    public string SubtotalText => $"{CurrencySymbol}{Subtotal:0.00}";
    public string DeliveryFeeText => $"{CurrencySymbol}{DeliveryFee:0.00}";

    /// <summary>When the order was cancelled (shown on Order details).</summary>
    [ObservableProperty]
    public partial DateTime? CancelledAt { get; set; }

    // ---------- Progress tracker (Order details) ----------
    /// <summary>Step 2 done once the cancel window has passed.</summary>
    public bool IsPreparing => !IsCancelled && !CanCancel;
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
/// Keeps the orders placed during this session (in memory, like MockDataService).
/// Register as a singleton in MauiProgram.cs.
/// </summary>
public class OrderHistoryService
{
    public ObservableCollection<PlacedOrder> Orders { get; } = new();

    public void Add(PlacedOrder order) => Orders.Insert(0, order);

    /// <summary>
    /// Cancel order (shared by the Orders list and Order details):
    /// asks first, marks it Cancelled and withdraws any Send-to-UK shipment.
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

        var confirm = await Shell.Current.DisplayAlertAsync("Cancel this order?",
            $"{order.OrderNumber} · {order.TotalText}\nYour {order.PaymentMethod} payment will be refunded.",
            "Cancel order", "Keep order");
        if (!confirm) return false;

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.Now;

        if (order.Shipment is not null)
            data.Shipments.Remove(order.Shipment);

        return true;
    }

    /// <summary>Buy again: put the same dishes and quantities back in the basket, then offer Checkout.</summary>
    public async Task BuyAgainAsync(PlacedOrder order, CartService cart)
    {
        foreach (var line in order.Lines)
            for (var i = 0; i < line.Quantity; i++)
                cart.AddDish(line.Dish);

        var go = await Shell.Current.DisplayAlertAsync("Added to your basket",
            $"{order.ItemCount} item(s) from {order.OrderNumber} are back in your basket.",
            "Checkout", "Keep shopping");
        if (go)
            await Shell.Current.GoToAsync(nameof(Views.CheckoutPage));
    }
}
