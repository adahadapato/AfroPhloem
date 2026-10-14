using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Services;

namespace Phloem.ViewModels;

/// <summary>Change password (route "changepassword").</summary>
public partial class ChangePasswordViewModel : ObservableObject
{
    private readonly SessionService _session;

    public ChangePasswordViewModel(SessionService session)
    {
        _session = session;
    }

    [ObservableProperty]
    public partial string CurrentPassword { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMinLength), nameof(HasNumber), nameof(HasLetter))]
    public partial string NewPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>The eye icon: shows / hides all three boxes.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HidePasswords))]
    public partial bool ShowPasswords { get; set; }

    public bool HidePasswords => !ShowPasswords;

    // Live checklist under "New password"
    public bool HasMinLength => (NewPassword ?? string.Empty).Length >= 8;
    public bool HasNumber    => (NewPassword ?? string.Empty).Any(char.IsDigit);
    public bool HasLetter    => (NewPassword ?? string.Empty).Any(char.IsLetter);

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsError { get; set; }

    /// <summary>Called by the page each time it appears: start with empty boxes.</summary>
    public void Reset()
    {
        CurrentPassword = NewPassword = ConfirmPassword = string.Empty;
        ShowPasswords = false;
        StatusMessage = string.Empty;
        IsError = false;
    }

    [RelayCommand]
    private void ToggleShow() => ShowPasswords = !ShowPasswords;

    [RelayCommand]
    private async Task Save()
    {
        var user = _session.CurrentUser;
        if (user is null) return;

        string? problem =
            string.IsNullOrEmpty(CurrentPassword)        ? "Enter your current password." :
            CurrentPassword != user.Password             ? "Your current password isn't right." :
            !HasMinLength || !HasNumber || !HasLetter    ? "Your new password needs at least 8 characters, with letters and a number." :
            NewPassword == CurrentPassword               ? "Your new password must be different from the current one." :
            NewPassword != ConfirmPassword               ? "The two new passwords don't match." :
            null;

        if (problem is not null)
        {
            IsError = true;
            StatusMessage = problem;
            return;
        }

        // DEMO ONLY: plaintext, like the rest of the mock data. Your backend should hash it.
        user.Password = NewPassword;

        await Shell.Current.DisplayAlert("Password changed", "Use your new password next time you log in.", "OK");
        await Shell.Current.GoToAsync("..");
    }
}
