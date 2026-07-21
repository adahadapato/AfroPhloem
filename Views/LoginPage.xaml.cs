using AfroPhloem.ViewModels;

namespace AfroPhloem.Views;

/// <summary>
/// class LoginPage
/// </summary>
public partial class LoginPage : ContentPage
{
    /// <summary>
    /// constructor for LoginPage
    /// </summary>
    /// <param name="vm"></param>
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}