using AfroPhloem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AfroPhloem.ViewModels;

/// <summary>
/// contains the properties and commands for the RegisterPage view.
/// </summary>
public partial class RegisterViewModel : ObservableObject
{
    private readonly MockDataService _data;
    private readonly CartService _cart;
    private readonly SessionService _session;

    /// <summary>
    /// property for the user's full name.
    /// </summary>
    [ObservableProperty]
    public partial string FullName { get; set; } = string.Empty;

    /// <summary>
    /// property for the user's email address.
    /// </summary>
    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    /// <summary>
    /// property for the user's phone number.
    /// </summary>
    [ObservableProperty]
    public partial string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// property for the user's delivery address.
    /// </summary>
    [ObservableProperty]
    public partial string Address { get; set; } = string.Empty;

    /// <summary>
    /// property for the user's password.
    /// </summary>
    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    /// <summary>
    /// property indicating whether the user has agreed to the terms and conditions.
    /// </summary>
    [ObservableProperty]
    public partial bool AgreedToTerms { get; set; }

    /// <summary>
    /// property for the status message to display to the user.
    /// </summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// property indicating whether there is an error in the registration process.
    /// </summary>
    [ObservableProperty]
    public partial bool IsError { get; set; }

    /// <summary>
    /// creates a new instance of the RegisterViewModel class, initializing it with
    /// the mock data service, cart service, and session service dependencies.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="cart"></param>
    /// <param name="session"></param>
    public RegisterViewModel(MockDataService data, CartService cart, SessionService session)
    {
        _data = data;
        _cart = cart;
        _session = session;
    }

    /// <summary>
    /// command to handle the sign-up process when the user clicks the sign-up button.
    /// Validates the form, persists the new account, and signs the user in automatically.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task SignUp()
    {
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            IsError = true;
            StatusMessage = "Please fill in all required fields.";
            return;
        }

        if (!AgreedToTerms)
        {
            IsError = true;
            StatusMessage = "Please accept the Terms & Conditions.";
            return;
        }

        if (_data.Users.Any(u => string.Equals(u.Email, Email, StringComparison.OrdinalIgnoreCase)))
        {
            IsError = true;
            StatusMessage = "An account with that email already exists.";
            return;
        }

        // No real backend/auth yet - this is a mock sign-up for demo purposes.
        var user = _data.RegisterUser(FullName, Email, PhoneNumber, Address, _cart.SelectedCountry?.Name ?? "Ghana", Password);

        // Sign the person straight in, same as most modern sign-up flows.
        _session.CurrentUser = user;

        IsError = false;
        StatusMessage = $"Welcome, {FullName}! Your account is ready.";
        await Shell.Current.GoToAsync("//home");
    }
}