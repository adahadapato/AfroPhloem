using Phloem.ViewModels;

namespace Phloem.Views;

public partial class CheckoutPage : ContentPage
{
    readonly CheckoutViewModel _vm;

    public CheckoutPage(CheckoutViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Totals, address and flag can change while you're on other pages
        _vm.RefreshAll();
    }

    // ☰ opens the side menu
    void OnMenuTapped(object? sender, TappedEventArgs e)
    {
        if (Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = true;
    }
}
