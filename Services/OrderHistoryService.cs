using System.Collections.ObjectModel;

namespace Phloem.Services;

/// <summary>A placed order, shown under "Recent orders" on the Orders page.</summary>
public class PlacedOrder
{
    public required string OrderNumber { get; init; }
    public DateTime PlacedAt { get; init; } = DateTime.Now;

    /// <summary>e.g. "Jollof Rice ×2, Egusi Soup ×1"</summary>
    public string ItemsSummary { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public decimal Total { get; init; }
    public string CurrencySymbol { get; init; } = "£";
    public string PaymentMethod { get; init; } = string.Empty;

    /// <summary>True when the order included a "Send to UK" shipment.</summary>
    public bool IsInternational { get; init; }

    public string StatusText => IsInternational ? "Shipping to UK" : "Order placed";
    public string TotalText => $"{CurrencySymbol}{Total:0.00}";
}

/// <summary>
/// Keeps the orders placed during this session (in memory, like MockDataService).
/// Register as a singleton in MauiProgram.cs.
/// </summary>
public class OrderHistoryService
{
    public ObservableCollection<PlacedOrder> Orders { get; } = new();

    public void Add(PlacedOrder order) => Orders.Insert(0, order);
}
