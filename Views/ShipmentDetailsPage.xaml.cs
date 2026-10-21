using Phloem.ViewModels;

namespace Phloem.Views;

public partial class ShipmentDetailsPage : ContentPage
{
    readonly ShipmentDetailsViewModel _vm;

    public ShipmentDetailsPage(ShipmentDetailsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // The parcel as it is now (its status may have moved on)
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _vm.LoadAsync(); }
        catch (Exception) { /* the page says it couldn't load */ }
    }

    async void OnBackTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");
}
