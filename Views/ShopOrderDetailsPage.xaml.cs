using Phloem.ViewModels;

namespace Phloem.Views;

public partial class ShopOrderDetailsPage : ContentPage
{
    readonly ShopOrderDetailsViewModel _vm;

    public ShopOrderDetailsPage(ShopOrderDetailsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // The order as it is now (the customer may have cancelled meanwhile)
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _vm.LoadAsync(); }
        catch (Exception) { /* the page says what went wrong; pull down to try again */ }
    }

    // ← back to the Seller dashboard
    async void OnBackTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");
}
