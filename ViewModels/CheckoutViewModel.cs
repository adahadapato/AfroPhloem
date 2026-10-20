using Phloem.Models;
using Phloem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace Phloem.ViewModels;

/// <summary>
/// organizes and manages the checkout process, 
/// including cart items, payment methods, and order placement.
/// </summary>
public partial class CheckoutViewModel : ObservableObject
{
    private readonly CartService _cart;
    private readonly MockDataService _data;
    private readonly SessionService _session;
    private readonly OrderHistoryService _history;
    private readonly DeliveryAddressService _delivery;

    /// <summary>
    /// observable collection of cart items, reflecting 
    /// the current state of the user's shopping cart.
    /// </summary>
    public ObservableCollection<CartItem> Items => _cart.Items;

    /// <summary>
    /// observable collection of available payment methods, 
    /// allowing users to select their preferred option for completing the purchase.
    /// </summary>
    public ObservableCollection<PaymentMethodOption> PaymentMethods { get; } = new()
    {
        new PaymentMethodOption { Name = "Credit / debit card", Subtitle = "Ending 3281", Icon = "💳" },
        new PaymentMethodOption { Name = "Mobile money", Subtitle = "MTN, Vodafone, Airtel", Icon = "📱" },
        new PaymentMethodOption { Name = "Bank transfer", Subtitle = "Direct transfer", Icon = "🏦" },
    };

    /// <summary>
    /// property representing the 
    /// currently selected payment method,
    /// </summary>
    [ObservableProperty]
    public partial PaymentMethodOption SelectedPaymentMethod { get; set; }

    /// <summary>
    /// property indicating whether the order has been placed,
    /// </summary>
    [ObservableProperty]
    public partial bool OrderPlaced { get; set; }

    /// <summary>
    /// property holding the confirmation message 
    /// displayed to the user after placing an order,
    /// </summary>
    [ObservableProperty]
    public partial string ConfirmationMessage { get; set; } = string.Empty;

    /// <summary>
    /// observable property storing the order 
    /// number generated upon order placement,
    /// </summary>
    [ObservableProperty]
    public partial string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets the currency symbol for the selected country.
    /// </summary>
    public string CurrencySymbol => _cart.SelectedCountry?.CurrencySymbol ?? "£";

    /// <summary>
    /// Gets the subtotal amount for all items in the cart.
    /// </summary>
    public decimal Subtotal => _cart.Subtotal;

    /// <summary>
    /// Gets the delivery fee for the current cart.
    /// </summary>
    public decimal DeliveryFee => _cart.DeliveryFee;

    /// <summary>
    /// Gets the total amount for all items in the shopping cart.
    /// </summary>
    public decimal Total => _cart.Total;

    /// <summary>
    /// Gets the total quantity of all items in the cart.
    /// </summary>
    public int ItemCount => _cart.Items.Sum(i => i.Quantity);

    /// <summary>
    /// Gets a value indicating whether the cart has a pending international delivery.
    /// </summary>
    public bool HasInternationalDelivery => _cart.PendingInternationalDelivery is not null;

    /// <summary>
    /// Gets the pending international delivery order.
    /// </summary>
    public DeliveryOrder? InternationalDelivery => _cart.PendingInternationalDelivery;

    /// <summary>
    /// Gets the country currently selected in the shopping cart.
    /// </summary>
    public CountryOption SelectedCountry => _cart.SelectedCountry;

    /// <summary>
    /// NEW: the line shown in the "Delivery address" card, e.g. "Home · 31 Camden High St, London".
    /// Uses the "Send to UK" recipient when there is one, otherwise the signed-in user's default address.
    /// </summary>
    public string DeliveryAddressText
    {
        get
        {
            if (_cart.PendingInternationalDelivery is { } intl)
                return $"{intl.RecipientName} · {intl.UkAddress}";

            // The address picked on the Delivery address page, or the user's default
            return _delivery.Describe(_session.IsLoggedIn ? _session.CurrentUser : null);
        }
    }

    /// <summary>
    /// creates a new instance of the CheckoutViewModel class, initializing it with 
    /// the provided cart service and setting up event handlers for cart item changes.
    /// </summary>
    public CheckoutViewModel(CartService cart, MockDataService data, SessionService session,
                             OrderHistoryService history, DeliveryAddressService delivery)
    {
        _cart = cart;
        _data = data;
        _session = session;
        _history = history;
        _delivery = delivery;
        SelectedPaymentMethod = PaymentMethods[0];
        _cart.Items.CollectionChanged += (_, _) => RefreshTotals();
    }

    /// <summary>
    /// refreshes the totals for subtotal, delivery fee, 
    /// total amount, and item count by notifying property changes.
    /// </summary>
    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(DeliveryFee));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(ItemCount));
    }

    /// <summary>
    /// NEW: called by the page each time it appears, so totals, the delivery address,
    /// the "Send to UK" details and the flag are always up to date.
    /// </summary>
    public void RefreshAll()
    {
        RefreshTotals();
        OnPropertyChanged(nameof(DeliveryAddressText));
        OnPropertyChanged(nameof(HasInternationalDelivery));
        OnPropertyChanged(nameof(InternationalDelivery));
        OnPropertyChanged(nameof(SelectedCountry));
        OnPropertyChanged(nameof(CurrencySymbol));
    }

    /// <summary>
    /// command method to increase the quantity of a specified cart item,
    /// </summary>
    [RelayCommand]
    private void IncreaseQuantity(CartItem item)
    {
        item.Quantity++;
        RefreshTotals();
    }

    /// <summary>
    /// command method to decrease the quantity of a specified cart item.
    /// </summary>
    [RelayCommand]
    private void DecreaseQuantity(CartItem item)
    {
        if (item.Quantity > 1)
            item.Quantity--;
        else
            _cart.RemoveItem(item);

        RefreshTotals();
    }

    /// <summary>
    /// "Change delivery details →". Goes back to the Send to UK form for international
    /// orders, otherwise to the Delivery address page (pick a saved address or add one).
    /// </summary>
    [RelayCommand]
    private async Task ChangeDeliveryDetails()
        => await Shell.Current.GoToAsync(HasInternationalDelivery ? "//home/international" : "address");

    /// <summary>
    /// command method to place the order.
    /// </summary>
    [RelayCommand]
    private async Task PlaceOrder()
    {
        try
        {
            if (SelectedPaymentMethod is null || _cart.Items.Count == 0) return;

            // Take everything we need BEFORE the basket is cleared
            var subtotal = Subtotal;
            var fee = DeliveryFee;
            var total = Total;
            var address = DeliveryAddressText;
            var itemCount = ItemCount;
            var currency = CurrencySymbol;
            var payment = SelectedPaymentMethod.Name;
            var lines = _cart.Items.Where(i => i.Dish is not null)
                                       .Select(i => new OrderLine(i.Dish, i.Quantity)).ToList();
            var summary = string.Join(", ", lines.Select(l => $"{l.Dish.Name} ×{l.Quantity}"));
            var shipment = _cart.PendingInternationalDelivery;

            OrderNumber = $"AP-{DateTime.Now:yyMMdd}-{Random.Shared.Next(1000, 9999)}";
            ConfirmationMessage = $"Your order has been placed and {payment} will be charged {currency}{total:0.00}. " +
                                  $"You can cancel it from Orders within {PlacedOrder.CancelWindow.TotalMinutes:0} minutes.";

            _history.Add(new PlacedOrder
            {
                OrderNumber = OrderNumber,
                ItemsSummary = summary,
                ItemCount = itemCount,
                Subtotal = subtotal,
                DeliveryFee = fee,
                Total = total,
                DeliveryAddress = address,
                CurrencySymbol = currency,
                PaymentMethod = payment,
                Lines = lines,      // for "Buy again"
                Shipment = shipment    // so "Cancel order" can withdraw it
            });

            if (shipment is not null)
                _data.AddShipment(shipment);

            _cart.Clear();
            OrderPlaced = true;
            RefreshAll();
        }
        catch (Exception ex)
        {
            // Show the real error instead of crashing
            await Shell.Current.DisplayAlertAsync("Couldn't place order", ex.ToString(), "OK");
        }
    }

    /// <summary>
    /// command method to navigate back to the shopping listing page after an order has been placed.
    /// </summary>
    [RelayCommand]
    private async Task BackToShopping()
    {
        OrderPlaced = false;
        await Shell.Current.GoToAsync("//listing");
    }
}
