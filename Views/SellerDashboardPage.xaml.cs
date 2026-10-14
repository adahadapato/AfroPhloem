using Phloem.ViewModels;

namespace Phloem.Views;

public partial class SellerDashboardPage : ContentPage
{
    readonly SellerDashboardViewModel _vm;

    public SellerDashboardPage(SellerDashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // Flag of the country picked on Home
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Reload();   // shows dishes just added, edited or deleted
        var cart = IPlatformApplication.Current?.Services.GetService<Phloem.Services.CartService>();
        FlagLabel.Text = cart?.SelectedCountry?.FlagEmoji ?? "🌍";
    }

    // ☰ opens the side menu
    void OnMenuTapped(object? sender, TappedEventArgs e)
    {
        if (Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = true;
    }
}
