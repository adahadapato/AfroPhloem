using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Services;

namespace Phloem.ViewModels;

/// <summary>
/// Edit profile (route "editprofile"): the signed-in person's own details.
/// Business details (shop name, business email/phone) live in Shop settings instead.
/// </summary>
public partial class EditProfileViewModel : ObservableObject
{
    private readonly SessionService _session;

    public EditProfileViewModel(SessionService session)
    {
        _session = session;
    }

    public IReadOnlyList<string> Countries { get; } = new[] { "Ghana", "Nigeria", "United Kingdom" };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Initials))]
    public partial string FullName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PhoneNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Country { get; set; }

    /// <summary>Shown read-only: changing an email needs a verification step on the backend.</summary>
    public string Email => _session.CurrentUser?.Email ?? string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPhoto), nameof(HasNoPhoto))]
    public partial string? PhotoPath { get; set; }

    public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoPath);
    public bool HasNoPhoto => !HasPhoto;

    public string Initials
    {
        get
        {
            var parts = (FullName ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => "?",
                1 => parts[0][..1].ToUpper(),
                _ => $"{parts[0][0]}{parts[^1][0]}".ToUpper()
            };
        }
    }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsError { get; set; }

    /// <summary>Called by the page each time it appears.</summary>
    public void Load()
    {
        var user = _session.CurrentUser;
        if (user is null) return;
        FullName = user.FullName;
        PhoneNumber = user.PhoneNumber ?? string.Empty;
        Country = Countries.Contains(user.Country) ? user.Country : Countries[0];
        PhotoPath = user.PhotoPath;
        StatusMessage = string.Empty;
        IsError = false;
        OnPropertyChanged(nameof(Email));
    }

    // ---------- Photo ----------
    [RelayCommand]
    private async Task PhotoOptions()
    {
        var choice = await Shell.Current.DisplayActionSheetAsync("Profile photo", "Cancel", null,
            "📷  Take a photo", "🖼  Choose from gallery");
        try
        {
            if (choice?.Contains("Take") == true)
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await Shell.Current.DisplayAlertAsync("Camera", "This device can't take photos. Choose one from your gallery instead.", "OK");
                    return;
                }
                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo is not null) PhotoPath = photo.FullPath;   // the backend will upload this later
            }
            else if (choice?.Contains("gallery") == true)
            {
                var photo = (await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 }))?.FirstOrDefault();
                if (photo is not null) PhotoPath = photo.FullPath;   // the backend will upload this later
            }
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlertAsync("Permission needed", "Please allow Phloem to use the camera and photos in your phone settings.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Couldn't get photo", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void RemovePhoto() => PhotoPath = null;

    // ---------- Save ----------
    [RelayCommand]
    private async Task Save()
    {
        var user = _session.CurrentUser;
        if (user is null) return;

        var name = (FullName ?? string.Empty).Trim();
        var phone = (PhoneNumber ?? string.Empty).Trim();

        if (name.Length < 2)
        {
            IsError = true;
            StatusMessage = "Please enter your full name.";
            return;
        }

        var digits = phone.Count(char.IsDigit);
        if (phone.Length > 0 && (digits < 7 || phone.Any(c => !(char.IsDigit(c) || c is '+' or ' ' or '-' or '(' or ')'))))
        {
            IsError = true;
            StatusMessage = "That phone number doesn't look right. Use digits, spaces and +, e.g. +233 24 555 0192.";
            return;
        }

        user.FullName = name;
        user.PhoneNumber = phone;
        user.Country = Country ?? user.Country;
        user.PhotoPath = PhotoPath;          // local file for now; the backend will upload it later

        IsError = false;
        StatusMessage = "Profile saved.";
        await Shell.Current.GoToAsync("..");
    }
}
