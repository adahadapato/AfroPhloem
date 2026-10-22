using Phloem.ViewModels;

namespace Phloem.Views;

public partial class ShopSettingsPage : ContentPage
{
    readonly ShopSettingsViewModel _vm;

    public ShopSettingsPage(ShopSettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // The shop as it is now (approval, documents reviewed)
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _vm.LoadAsync(); }
        catch (Exception) { /* the page says what went wrong; pull down to try again */ }
    }

    async void OnBackTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");
}
