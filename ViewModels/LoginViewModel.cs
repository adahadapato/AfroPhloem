using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Services;

namespace Phloem.ViewModels;

/// <summary>
/// Represents the view model for user login, providing properties and commands for authentication and navigation.
/// </summary>
/// <remarks>
/// <para>Signs in through the Phloem API (<see cref="AuthService"/>). When the account has two-step verification on,
/// the 6-digit code (or a recovery code) is asked for in a pop-up, then sign-in finishes.</para>
/// <para>"Remember me" keeps the user signed in after the app is closed.</para>
/// </remarks>
public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _auth;

    /// <summary>
    /// property for the user's email address, which is observable and can be bound to the UI.
    /// </summary>
    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    /// <summary>
    /// property for the user's password, which is observable and can be bound to the UI.
    /// </summary>
    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    /// <summary>
    /// property indicating whether the user wants to stay signed in after closing the app.
    /// </summary>
    [ObservableProperty]
    public partial bool RememberMe { get; set; } = true;

    /// <summary>
    /// property for the status message displayed to the user, which is observable and can be bound to the UI.
    /// </summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// property indicating whether there is an error state, which is observable and can be bound to the UI.
    /// </summary>
    [ObservableProperty]
    public partial bool IsError { get; set; }

    /// <summary>
    /// creates a new instance of the LoginViewModel class with the service that signs in through the API.
    /// </summary>
    /// <param name="auth">Signs in and out through the Phloem API.</param>
    public LoginViewModel(AuthService auth)
    {
        _auth = auth;
    }

    /// <summary>
    /// command that signs in through the API (asking for the two-step code when needed) and goes to Home.
    /// The button is disabled while it runs, so it can't be pressed twice.
    /// </summary>
    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Please enter your email and password.");
            return;
        }

        ShowInfo("Signing in…");
        var login = await _auth.LoginAsync(Email, Password, RememberMe);
        if (login.IsFailure)
        {
            ShowError(login.Error!.DisplayMessage);
            return;
        }

        // Two-step verification: ask for the code, then finish signing in
        if (login.Value!.RequiresTwoFactor)
        {
            var code = await Shell.Current.DisplayPromptAsync(
                "Two-step verification",
                "Enter the 6-digit code from your authenticator app, or one of your recovery codes.",
                accept: "Verify", cancel: "Cancel", placeholder: "123456", maxLength: 20);

            if (string.IsNullOrWhiteSpace(code))
            {
                ShowError("Sign-in cancelled.");
                return;
            }

            // A 6-digit number is an authenticator code; anything else is a recovery code
            var trimmed = code.Trim();
            var isRecoveryCode = !(trimmed.Length == 6 && trimmed.All(char.IsDigit));

            var verified = await _auth.CompleteTwoFactorAsync(login.Value.TwoFactorChallengeToken!, trimmed,
                                                              isRecoveryCode, RememberMe);
            if (verified.IsFailure)
            {
                ShowError(verified.Error!.DisplayMessage);
                return;
            }
        }

        Password = string.Empty;
        ShowInfo("Welcome back!");
        // Back to Checkout if that's where they were heading; otherwise close Log in and go Home
        if (await AppNav.ResumeAfterLoginAsync()) return;
        await Shell.Current.Navigation.PopToRootAsync(false);
        await Shell.Current.GoToAsync("//home");
    }

    /// <summary>
    /// command that navigates the user to the registration page, allowing them to create a new account.
    /// </summary>
    [RelayCommand]
    private async Task GoToRegister() => await Shell.Current.GoToAsync("//account/register");

    /// <summary>
    /// command that asks the API to email a password-reset link to the email typed above.
    /// </summary>
    [RelayCommand]
    private async Task ForgotPassword()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ShowError("Type your email address above first, then tap \"Forgot password?\".");
            return;
        }

        var sent = await _auth.ForgotPasswordAsync(Email);
        if (sent.IsFailure)
        {
            ShowError(sent.Error!.DisplayMessage);
            return;
        }

        // Same answer whether or not the account exists (nobody can use this to find accounts)
        ShowInfo("If an account exists for that email, a reset link has been sent.");
    }

    private void ShowError(string message)
    {
        IsError = true;
        StatusMessage = message;
    }

    private void ShowInfo(string message)
    {
        IsError = false;
        StatusMessage = message;
    }
}
