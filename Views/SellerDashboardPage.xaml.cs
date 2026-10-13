using Phloem.ViewModels;

namespace Phloem.Views;

public partial class SellerDashboardPage : ContentPage
{
    public SellerDashboardPage(SellerDashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    // Flag of the country picked on Home
    protected override void OnAppearing()
    {
        base.OnAppearing();
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
