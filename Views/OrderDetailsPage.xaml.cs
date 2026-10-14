using Phloem.ViewModels;

namespace Phloem.Views;

public partial class OrderDetailsPage : ContentPage
{
    readonly OrderDetailsViewModel _vm;

    public OrderDetailsPage(OrderDetailsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Refresh();   // the 30-minute cancel window may have passed
    }

    // ← back to Orders
    async void OnBackTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("..");
}
