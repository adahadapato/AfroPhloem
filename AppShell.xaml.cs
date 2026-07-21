namespace AfroPhloem;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(Views.CheckoutPage), typeof(Views.CheckoutPage));
    }
}
