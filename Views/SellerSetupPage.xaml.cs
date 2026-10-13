using Phloem.ViewModels;

namespace Phloem.Views;

public partial class SellerSetupPage : ContentPage
{
    readonly SellerSetupViewModel _vm;

    public SellerSetupPage(SellerSetupViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Re-check sign-in each time, so the notice is right after logging in / signing up
        _vm.RefreshSession();

        // Flag of the country picked on Home
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
