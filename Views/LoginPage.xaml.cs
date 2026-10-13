using Phloem.ViewModels;

namespace Phloem.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    // Show the flag of the country picked on Home
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

    // Eye icon: show / hide the password
    void OnTogglePasswordTapped(object? sender, TappedEventArgs e)
        => PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
}
