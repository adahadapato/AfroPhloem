using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfroPhloem.Models;
using AfroPhloem.Services;

namespace AfroPhloem.ViewModels;

public partial class CheckoutViewModel : ObservableObject
{
    private readonly CartService _cart;

    public ObservableCollection<CartItem> Items => _cart.Items;

    public ObservableCollection<PaymentMethodOption> PaymentMethods { get; } = new()
    {
        new PaymentMethodOption { Name = "Card", Subtitle = "Ending 3281", Icon = "💳" },
        new PaymentMethodOption { Name = "Mobile Money", Subtitle = "MTN, Vodafone, Airtel", Icon = "📱" },
        new PaymentMethodOption { Name = "Bank Transfer", Subtitle = "Direct transfer", Icon = "🏦" },
    };

    [ObservableProperty]
    private PaymentMethodOption selectedPaymentMethod;

    [ObservableProperty]
    private bool orderPlaced;

    [ObservableProperty]
    private string confirmationMessage = string.Empty;

    [ObservableProperty]
    private string orderNumber = string.Empty;

    public string CurrencySymbol => _cart.SelectedCountry.CurrencySymbol;
    public decimal Subtotal => _cart.Subtotal;
    public decimal DeliveryFee => _cart.DeliveryFee;
    public decimal Total => _cart.Total;
    public int ItemCount => _cart.Items.Sum(i => i.Quantity);
    public bool HasInternationalDelivery => _cart.PendingInternationalDelivery is not null;
    public DeliveryOrder? InternationalDelivery => _cart.PendingInternationalDelivery;

    public CountryOption SelectedCountry => _cart.SelectedCountry;
    public CheckoutViewModel(CartService cart)
    {
        _cart = cart;
        selectedPaymentMethod = PaymentMethods[0];
        _cart.Items.CollectionChanged += (_, _) => RefreshTotals();
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(DeliveryFee));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(ItemCount));
    }

    [RelayCommand]
    private void IncreaseQuantity(CartItem item)
    {
        item.Quantity++;
        RefreshTotals();
    }

    [RelayCommand]
    private void DecreaseQuantity(CartItem item)
    {
        if (item.Quantity > 1)
            item.Quantity--;
        else
            _cart.RemoveItem(item);

        RefreshTotals();
    }

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

    [RelayCommand]
    private async Task BackToShopping()
    {
        OrderPlaced = false;
        await Shell.Current.GoToAsync("//listing");
    }
}