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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Load();
    }

    async void OnBackTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");
}
