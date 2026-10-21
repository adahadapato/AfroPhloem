using Phloem.ViewModels;

namespace Phloem.Views;

public partial class InternationalDeliveryPage : ContentPage
{
    readonly InternationalDeliveryViewModel _vm;

    public InternationalDeliveryPage(InternationalDeliveryViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // The routes from the server, and the sender's phone from their profile
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _vm.LoadAsync(); }
        catch (Exception) { /* the page says it couldn't load; the user can come back to it */ }
    }

    // ☰ opens the side menu
    void OnMenuTapped(object? sender, TappedEventArgs e)
    {
        if (Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = true;
    }
}
