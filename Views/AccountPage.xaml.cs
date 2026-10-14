using Phloem.ViewModels;

namespace Phloem.Views;

public partial class AccountPage : ContentPage
{
    readonly AccountViewModel _vm;

    public AccountPage(AccountViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Load();   // someone may have just logged in, signed up or opened a shop
    }

    // ☰ opens the side menu
    void OnMenuTapped(object? sender, TappedEventArgs e)
    {
        if (Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = true;
    }
}
