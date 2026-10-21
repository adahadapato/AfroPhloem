using Phloem.ViewModels;

namespace Phloem.Views;

public partial class AdminShipmentsPage : ContentPage
{
    readonly AdminShipmentsViewModel _vm;

    public AdminShipmentsPage(AdminShipmentsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // Fresh list each time (staff may have moved parcels on)
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _vm.LoadAsync(); }
        catch (Exception) { /* the page says what went wrong; pull down to try again */ }
    }

    async void OnBackTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");
}
