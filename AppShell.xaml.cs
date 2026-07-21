namespace AfroPhloem;

/// <summary>
/// class representing the main application shell,
/// </summary>
public partial class AppShell : Shell
{
    /// <summary>
    /// constructor for the AppShell class, initializing 
    /// the shell and registering routes for navigation.
    /// </summary>
    public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(Views.CheckoutPage), typeof(Views.CheckoutPage));
    }
}
