using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AfroPhloem.ViewModels;

/// <summary>
/// Represents the view model for user login, providing properties and commands for authentication and navigation.
/// </summary>
/// <remarks>Includes observable properties for email, password, remember-me selection, status messages, and error
/// state. Exposes commands for login, registration navigation, and password recovery.</remarks>
public partial class LoginViewModel : ObservableObject
{
    /// <summary>
    /// property for the user's email address, which is observable and can be bound to the UI.
    /// </summary>
    [ObservableProperty]
    private string email = string.Empty;

    /// <summary>
    /// property for the user's password, which is observable and can be bound to the UI.
    /// </summary>
    [ObservableProperty]
    private string password = string.Empty;

    /// <summary>
    /// property indicating whether the user wants to be remembered for future logins, which is observable and can be bound to the UI.
    /// </summary>
    [ObservableProperty]
    private bool rememberMe;

    /// <summary>
    /// property for the status message displayed to the user, which is observable and can be bound to the UI.
    /// </summary>
    [ObservableProperty]
    private string statusMessage = string.Empty;

    /// <summary>
    /// property indicating whether there is an error state, which is observable and can be bound to the UI.
    /// </summary>
    [ObservableProperty]
    private bool isError;

    /// <summary>
    /// command that handles the login process, validating user input and navigating to the home page upon successful login.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            IsError = true;
            StatusMessage = "Please enter your email and password.";
            return;
        }

        // No real backend/auth yet - this is a mock sign-in for demo purposes.
        IsError = false;
        StatusMessage = "Welcome back!";
        await Shell.Current.GoToAsync("//home");
    }

    /// <summary>
    /// command that navigates the user to the registration page, allowing them to create a new account.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task GoToRegister() => await Shell.Current.GoToAsync("//register");

    /// <summary>
    /// command that handles the password recovery process, providing 
    /// feedback to the user that a reset link has been sent if an account exists for the provided email.
    /// </summary>
    [RelayCommand]
    private void ForgotPassword()
    {
        IsError = false;
        StatusMessage = "If an account exists for that email, a reset link has been sent.";
    }
}