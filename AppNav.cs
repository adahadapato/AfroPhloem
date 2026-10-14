using System.Windows.Input;

namespace Phloem;

/// <summary>
/// App-wide navigation helpers that any page can use straight from XAML, e.g.
///   &lt;TapGestureRecognizer Command="{x:Static phloem:AppNav.GoHomeCommand}" /&gt;
/// </summary>
public static class AppNav
{
    /// <summary>Phloem logo tapped: go to the Home tab and close anything opened on top of it.</summary>
    public static ICommand GoHomeCommand { get; } = new Command(async () => await GoHomeAsync());

    public static async Task GoHomeAsync()
    {
        if (Shell.Current is null) return;

        Shell.Current.FlyoutIsPresented = false;
        await Shell.Current.GoToAsync("//home");

        // If pages were opened on top of Home (Vendors, Seller setup…), go back to Home itself
        if (Shell.Current.Navigation.NavigationStack.Count > 1)
            await Shell.Current.Navigation.PopToRootAsync(false);
    }
}
