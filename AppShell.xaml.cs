using Phloem.Views;

namespace Phloem;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages that open on top of a tab (so the bottom bar stays visible).
        // These names must NOT also be used as a ShellContent Route in AppShell.xaml.
        Routing.RegisterRoute("about", typeof(AboutPage));
        Routing.RegisterRoute("vendors", typeof(VendorsListPage));
        Routing.RegisterRoute("sellersetup", typeof(SellerSetupPage));
        Routing.RegisterRoute("dashboard", typeof(SellerDashboardPage));
        Routing.RegisterRoute("admin", typeof(AdminPage));
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("login", typeof(LoginPage));               // opened from My Account
        Routing.RegisterRoute("international", typeof(InternationalDeliveryPage)); // Send Food to UK
        Routing.RegisterRoute("address", typeof(DeliveryAddressPage));       // pick / add a delivery address
        Routing.RegisterRoute("orderdetails", typeof(OrderDetailsPage));          // one placed order
        Routing.RegisterRoute("dishedit", typeof(DishEditorPage));            // seller adds / edits a dish
        Routing.RegisterRoute("editprofile", typeof(EditProfilePage));           // My account → Edit profile
        Routing.RegisterRoute("changepassword", typeof(ChangePasswordPage));        // My account → Change password
        Routing.RegisterRoute("notifications", typeof(NotificationSettingsPage));  // My account → Notifications
        Routing.RegisterRoute("shopsettings", typeof(ShopSettingsPage));          // Seller dashboard → Shop settings



        // Checkout (was already reached with GoToAsync(nameof(CheckoutPage))).
        Routing.RegisterRoute(nameof(CheckoutPage), typeof(CheckoutPage));
    }

    // ------------------------------------------------------------------
    // Each bottom tab keeps its own stack of pages. Without this, leaving
    // Home while About/Vendors/etc. is open on it means tapping Home later
    // shows that old page instead of the Home page.
    // Fix: whenever we move to a different tab, quietly close the pages
    // left open on the tab we just left, so every tab reopens at its start.
    // ------------------------------------------------------------------
    ShellSection? _lastTab;

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        var currentTab = CurrentItem?.CurrentItem;   // the tab now on screen
        var previousTab = _lastTab;
        _lastTab = currentTab;

        if (previousTab is not null && previousTab != currentTab && previousTab.Stack.Count > 1)
        {
            // The tab we left isn't visible, so this happens without any flicker.
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try { await previousTab.Navigation.PopToRootAsync(false); }
                catch { /* nothing to close */ }
            });
        }
    }

    // Side-menu entries for the pages above: close the menu, then open the page
    // on its tab, e.g. "//account/about" = Account tab, then About on top of it.
    async void OnMenuItemClicked(object? sender, EventArgs e)
    {
        if (sender is MenuItem { CommandParameter: string route })
        {
            FlyoutIsPresented = false;
            await GoToAsync(route);
        }
    }
}
