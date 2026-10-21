using Phloem.ViewModels;

namespace Phloem.Views;

public partial class OrdersPage : ContentPage
{
    readonly OrdersViewModel _vm;

    public OrdersPage(OrdersViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.RefreshWithOrders();   // basket may have changed on another page; orders come from the server
    }

    // ☰ opens the side menu
    void OnMenuTapped(object? sender, TappedEventArgs e)
    {
        if (Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = true;
    }
}
