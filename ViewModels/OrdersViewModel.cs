using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;

namespace Phloem.ViewModels;

/// <summary>
/// The Orders page: the current basket (with Checkout / Continue shopping) and recent orders.
/// </summary>
public partial class OrdersViewModel : ObservableObject
{
    private readonly CartService _cart;
    private readonly OrderHistoryService _history;
    private readonly SessionService _session;
    private readonly DeliveryAddressService _delivery;
    private readonly MockDataService _data;

    public ObservableCollection<CartItem> Items => _cart.Items;
    public ObservableCollection<PlacedOrder> PastOrders => _history.Orders;

    public int ItemCount => _cart.Items.Sum(i => i.Quantity);
    public decimal Subtotal => _cart.Subtotal;
    public decimal DeliveryFee => _cart.DeliveryFee;
    public decimal Total => _cart.Total;
    public CountryOption SelectedCountry => _cart.SelectedCountry;

    public bool HasItems => ItemCount > 0;
    public bool IsBasketEmpty => !HasItems;
    public bool HasPastOrders => _history.Orders.Count > 0;

    /// <summary>"Home · 31 Camden High St, London" for the Deliver to card.</summary>
    public string DeliveryAddressText => _delivery.Describe(_session.IsLoggedIn ? _session.CurrentUser : null);

    public OrdersViewModel(CartService cart, OrderHistoryService history,
                           SessionService session, DeliveryAddressService delivery,
                           MockDataService data)
    {
        _cart = cart;
        _history = history;
        _session = session;
        _delivery = delivery;
        _data = data;
        _cart.Items.CollectionChanged += (_, _) => Refresh();
        _history.Orders.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasPastOrders));
    }

    /// <summary>Called by the page each time it appears.</summary>
    public void Refresh()
    {
        OnPropertyChanged(nameof(ItemCount));
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(DeliveryFee));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(IsBasketEmpty));
        OnPropertyChanged(nameof(HasPastOrders));
        OnPropertyChanged(nameof(SelectedCountry));
        OnPropertyChanged(nameof(DeliveryAddressText));

        // The 30-minute cancel window may have run out since last time
        foreach (var order in _history.Orders) order.RefreshTiming();
    }

    /// <summary>Called by the page each time it appears: the basket, then "Recent orders" fresh from the server.</summary>
    public async void RefreshWithOrders()
    {
        Refresh();
        try
        {
            await _history.LoadAsync();
        }
        catch (Exception)
        {
            // Couldn't reach the server: the orders shown last time stay
        }

        OnPropertyChanged(nameof(HasPastOrders));
    }

    [RelayCommand]
    private void IncreaseQuantity(CartItem item)
    {
        item.Quantity++;
        Refresh();
    }

    [RelayCommand]
    private void DecreaseQuantity(CartItem item)
    {
        if (item.Quantity > 1)
            item.Quantity--;
        else
            _cart.RemoveItem(item);
        Refresh();
    }

    [RelayCommand]
    private void RemoveItem(CartItem item)
    {
        _cart.RemoveItem(item);
        Refresh();
    }

    [RelayCommand]
    private async Task GoToCheckout()
    {
        if (!HasItems) return;
        await AppNav.GoToCheckoutAsync();
    }

    /// <summary>Cancel order: asks first, then marks the order Cancelled (and withdraws any UK shipment).</summary>
    [RelayCommand]
    private async Task CancelOrder(PlacedOrder order) => await _history.CancelAsync(order, _data);

    /// <summary>Buy again: put the same dishes and quantities back in the basket.</summary>
    [RelayCommand]
    private async Task BuyAgain(PlacedOrder order)
    {
        await _history.BuyAgainAsync(order, _cart);
        Refresh();
    }

    /// <summary>Order number tapped → Order details.</summary>
    [RelayCommand]
    private async Task OpenOrder(PlacedOrder order) =>
        await Shell.Current.GoToAsync("orderdetails", new Dictionary<string, object> { ["Order"] = order });

    /// <summary>"Change delivery details →" opens the Delivery address page.</summary>
    [RelayCommand]
    private async Task ChangeDeliveryAddress() => await Shell.Current.GoToAsync("address");

    [RelayCommand]
    private async Task ContinueShopping() => await Shell.Current.GoToAsync("//listing");
}
