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

        // Checkout (was already reached with GoToAsync(nameof(CheckoutPage))).
        Routing.RegisterRoute(nameof(CheckoutPage), typeof(CheckoutPage));
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
