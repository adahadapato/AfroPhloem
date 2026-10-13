using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;
using Phloem.Views;

namespace Phloem.ViewModels;

public partial class InternationalDeliveryViewModel : ObservableObject
{
    private readonly CartService _cart;

    public ObservableCollection<DeliverySpeed> SpeedOptions { get; } =
        new(Enum.GetValues<DeliverySpeed>());

    [ObservableProperty]
    public partial string SenderName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RecipientName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UkAddress { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ItemDescription { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DeliverySpeed SelectedSpeed { get; set; } = DeliverySpeed.Standard;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsError { get; set; }

    public decimal EstimatedFee => new DeliveryOrder { Speed = SelectedSpeed }.Fee;

    partial void OnSelectedSpeedChanged(DeliverySpeed value) => OnPropertyChanged(nameof(EstimatedFee));

    public InternationalDeliveryViewModel(CartService cart)
    {
        _cart = cart;
    }

    /// <summary>NEW: the Standard / Express / Bulk pills.</summary>
    [RelayCommand]
    private void SelectSpeed(DeliverySpeed speed) => SelectedSpeed = speed;

    [RelayCommand]
    private async Task ScheduleDelivery()
    {
        if (string.IsNullOrWhiteSpace(SenderName) || string.IsNullOrWhiteSpace(RecipientName) || string.IsNullOrWhiteSpace(UkAddress))
        {
            IsError = true;
            StatusMessage = "Sender name, recipient name, and UK address are required.";
            return;
        }

        _cart.PendingInternationalDelivery = new DeliveryOrder
        {
            SenderName = SenderName,
            RecipientName = RecipientName,
            UkAddress = UkAddress,
            ItemDescription = ItemDescription,
            Speed = SelectedSpeed,
            FromCountry = _cart.SelectedCountry?.Name ?? "Ghana"
        };

        IsError = false;
        StatusMessage = "Shipment details saved — continue to checkout.";
        await Shell.Current.GoToAsync(nameof(Views.CheckoutPage));
    }
}
