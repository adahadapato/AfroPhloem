using Phloem.ViewModels;

namespace Phloem.Views;

public partial class AdminShopDetailsPage : ContentPage
{
    readonly AdminShopDetailsViewModel _vm;

    public AdminShopDetailsPage(AdminShopDetailsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // The shop as it is now (documents may have been reviewed)
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _vm.LoadAsync(); }
        catch (Exception) { /* the page says what went wrong */ }
    }

    async void OnBackTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");
}
