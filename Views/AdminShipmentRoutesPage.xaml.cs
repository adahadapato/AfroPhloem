using Phloem.ViewModels;

namespace Phloem.Views;

public partial class AdminShipmentRoutesPage : ContentPage
{
    readonly AdminShipmentRoutesViewModel _vm;

    public AdminShipmentRoutesPage(AdminShipmentRoutesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // The routes as they are now
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _vm.LoadAsync(); }
        catch (Exception) { /* the page says what went wrong; pull down to try again */ }
    }

    async void OnBackTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");
}
