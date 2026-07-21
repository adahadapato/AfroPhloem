using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfroPhloem.Models;
using AfroPhloem.Services;

namespace AfroPhloem.ViewModels;

/// <summary>
/// organizes and manages the checkout process, 
/// including cart items, payment methods, and order placement.
/// </summary>
public partial class CheckoutViewModel : ObservableObject
{
    private readonly CartService _cart;

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
        new PaymentMethodOption { Name = "Card", Subtitle = "Ending 3281", Icon = "💳" },
        new PaymentMethodOption { Name = "Mobile Money", Subtitle = "MTN, Vodafone, Airtel", Icon = "📱" },
        new PaymentMethodOption { Name = "Bank Transfer", Subtitle = "Direct transfer", Icon = "🏦" },
    };

    /// <summary>
    /// property representing the 
    /// currently selected payment method,
    /// </summary>
    [ObservableProperty]
    private PaymentMethodOption selectedPaymentMethod;

    /// <summary>
    /// property indicating whether the order has been placed,
    /// </summary>
    [ObservableProperty]
    private bool orderPlaced;

    /// <summary>
    /// property holding the confirmation message 
    /// displayed to the user after placing an order,
    /// </summary>
    [ObservableProperty]
    private string confirmationMessage = string.Empty;

    /// <summary>
    /// observable property storing the order 
    /// number generated upon order placement,
    /// </summary>
    [ObservableProperty]
    private string orderNumber = string.Empty;

    /// <summary>
    /// Gets the currency symbol for the selected country.
    /// </summary>
    public string CurrencySymbol => _cart.SelectedCountry.CurrencySymbol;

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
    /// creates a new instance of the CheckoutViewModel class, initializing it with 
    /// the provided cart service and setting up event handlers for cart item changes.
    /// </summary>
    /// <param name="cart"></param>
    public CheckoutViewModel(CartService cart)
    {
        _cart = cart;
        selectedPaymentMethod = PaymentMethods[0];
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
    /// command method to increase the quantity of a specified cart item,
    /// </summary>
    /// <param name="item"></param>
    [RelayCommand]
    private void IncreaseQuantity(CartItem item)
    {
        item.Quantity++;
        RefreshTotals();
    }

    /// <summary>
    /// command method to decrease the quantity of a specified cart item.
    /// </summary>
    /// <param name="item"></param>
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
    /// command method to remove a specified cart item from the shopping cart.
    /// </summary>
    [RelayCommand]
    private void PlaceOrder()
    {
        if (SelectedPaymentMethod is null) return;

        OrderNumber = $"AP-{DateTime.Now:yyMMdd}-{Random.Shared.Next(1000, 9999)}";
        ConfirmationMessage = $"Your order has been placed and {SelectedPaymentMethod.Name} will be charged {CurrencySymbol}{Total:0.00}.";
        OrderPlaced = true;
        _cart.Clear();
        RefreshTotals();
    }


    /// <summary>
    /// command method to navigate back to the shopping listing page after an order has been placed.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task BackToShopping()
    {
        OrderPlaced = false;
        await Shell.Current.GoToAsync("//listing");
    }
}