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
                           SessionService session, DeliveryAddressService delivery)
    {
        _cart = cart;
        _history = history;
        _session = session;
        _delivery = delivery;
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
        await Shell.Current.GoToAsync(nameof(Views.CheckoutPage));
    }

    /// <summary>"Change delivery details →" opens the Delivery address page.</summary>
    [RelayCommand]
    private async Task ChangeDeliveryAddress() => await Shell.Current.GoToAsync("address");

    [RelayCommand]
    private async Task ContinueShopping() => await Shell.Current.GoToAsync("//listing");
}
