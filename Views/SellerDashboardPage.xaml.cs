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

    // Flag of the country picked on Home; the shop and its orders as they are now
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var cart = IPlatformApplication.Current?.Services.GetService<Phloem.Services.CartService>();
        FlagLabel.Text = cart?.SelectedCountry?.FlagEmoji ?? "🌍";
        try { await _vm.ReloadAsync(); }
        catch (Exception) { /* the page says it couldn't load; pull down to try again */ }
    }

    // ☰ opens the side menu
    void OnMenuTapped(object? sender, TappedEventArgs e)
    {
        if (Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = true;
    }
}
