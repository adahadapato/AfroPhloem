using Phloem.ViewModels;

namespace Phloem.Views;

public partial class DeliveryAddressPage : ContentPage
{
    readonly DeliveryAddressViewModel _vm;

    public DeliveryAddressPage(DeliveryAddressViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Load();   // signed-in user and their addresses may have changed
    }

    // ← back to Checkout / Orders
    async void OnBackTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("..");
}
