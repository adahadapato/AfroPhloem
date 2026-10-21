using Phloem.ViewModels;

namespace Phloem.Views;

public partial class MyShipmentsPage : ContentPage
{
    readonly MyShipmentsViewModel _vm;

    public MyShipmentsPage(MyShipmentsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // Statuses change as the parcel travels, so reload each time
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _vm.LoadAsync(); }
        catch (Exception) { /* the list says what went wrong; pull down to try again */ }
    }

    async void OnBackTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");
}
