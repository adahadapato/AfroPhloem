using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Services;

namespace Phloem.ViewModels;

/// <summary>
/// Order details page. Receives the order via navigation:
///   GoToAsync("orderdetails", new Dictionary&lt;string, object&gt; { ["Order"] = order })
/// </summary>
public partial class OrderDetailsViewModel : ObservableObject, IQueryAttributable
{
    private readonly OrderHistoryService _history;
    private readonly CartService _cart;
    private readonly MockDataService _data;

    [ObservableProperty]
    public partial PlacedOrder? Order { get; set; }

    public OrderDetailsViewModel(OrderHistoryService history, CartService cart, MockDataService data)
    {
        _history = history;
        _cart = cart;
        _data = data;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Order", out var value) && value is PlacedOrder order)
        {
            order.RefreshTiming();
            Order = order;
        }
    }

    /// <summary>Re-check the cancel window each time the page appears.</summary>
    public void Refresh() => Order?.RefreshTiming();

    [RelayCommand]
    private async Task CancelOrder()
    {
        if (Order is not null)
            await _history.CancelAsync(Order, _data);
    }

    [RelayCommand]
    private async Task BuyAgain()
    {
        if (Order is not null)
            await _history.BuyAgainAsync(Order, _cart);
    }
}
