using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Phloem.Models;
using Phloem.Services;
using Phloem.Services.Api.Contracts;
using Phloem.Services.Api.Contracts.Commands;
using Phloem.Settings;

namespace Phloem.ViewModels;

/// <summary>
/// Edit profile (route "editprofile"): the signed-in person's own details, saved to the API.
/// <list type="number">
///   <item><description><b>Photo:</b> uploaded (or removed) as soon as it's chosen.</description></item>
///   <item><description><b>Profile:</b> first name, last name, phone, country (<c>PUT /api/me</c>).</description></item>
///   <item><description><b>Personal details:</b> date of birth + home address (<c>PUT /api/me/personal-details</c>);
///   optional here, but needed before opening a shop or sending a parcel. Saved only when filled in and
///   changed.</description></item>
/// </list>
/// Business details (shop name, business email/phone) live in Shop settings instead.
/// </summary>
public partial class EditProfileViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly AccountService _account;
    private readonly AuthService _auth;
    private readonly ApiSettings _api;

    public EditProfileViewModel(SessionService session, AccountService account, AuthService auth,
                                IOptions<ApiSettings> api)
    {
        _session = session;
        _account = account;
        _auth = auth;
        _api = api.Value;
    }

    /// <summary>The countries from the API.</summary>
    public IReadOnlyList<CountryOption> Countries => CountryOption.All;

    // ---------- Profile ----------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Initials))]
    public partial string FirstName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Initials))]
    public partial string LastName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PhoneNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial CountryOption? Country { get; set; }

    /// <summary>Shown read-only: changing an email needs a verification step on the backend.</summary>
    public string Email => _session.Profile?.Email ?? _session.CurrentUser?.Email ?? string.Empty;

    // ---------- Personal details ----------

    [ObservableProperty]
    public partial DateTime? DateOfBirth { get; set; }

    [ObservableProperty]
    public partial string AddressLine1 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddressLine2 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string City { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RegionOrPostcode { get; set; } = string.Empty;

    /// <summary>The date picker can't go past today.</summary>
    public DateTime Today => DateTime.Today;

    /// <summary>"Needed before you can open a shop or send a parcel." or "Saved."</summary>
    public string PersonalDetailsNote => _session.Profile?.HasPersonalDetails == true
        ? "Saved. You can open a shop and send parcels."
        : "Needed before you can open a shop or send a parcel.";

    // ---------- Photo ----------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPhoto), nameof(HasNoPhoto))]
    public partial string? PhotoPath { get; set; }

    public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoPath);
    public bool HasNoPhoto => !HasPhoto;

    public string Initials
    {
        get
        {
            var first = (FirstName ?? string.Empty).Trim();
            var last = (LastName ?? string.Empty).Trim();
            var initials = $"{(first.Length > 0 ? first[0] : ' ')}{(last.Length > 0 ? last[0] : ' ')}".Trim();
            return initials.Length == 0 ? "?" : initials.ToUpper();
        }
    }

    // ---------- Status ----------

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsError { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    /// <summary>Called by the page each time it appears: the server's copy of the details.</summary>
    public void Load()
    {
        var profile = _session.Profile;
        if (profile is null) return;

        OnPropertyChanged(nameof(Countries));   // before choosing one, so the picker keeps the choice
        FirstName = profile.FirstName;
        LastName = profile.LastName;
        PhoneNumber = profile.PhoneNumber ?? string.Empty;
        Country = Countries.FirstOrDefault(c => c.ApiCountry == profile.Country) ?? Countries.FirstOrDefault();
        PhotoPath = _api.ResolveFileUrl(profile.PhotoUrl);

        DateOfBirth = profile.DateOfBirth?.ToDateTime(TimeOnly.MinValue);
        AddressLine1 = profile.HomeAddress?.Line1 ?? string.Empty;
        AddressLine2 = profile.HomeAddress?.Line2 ?? string.Empty;
        City = profile.HomeAddress?.City ?? string.Empty;
        RegionOrPostcode = profile.HomeAddress?.RegionOrPostcode ?? string.Empty;

        StatusMessage = string.Empty;
        IsError = false;
        OnPropertyChanged(nameof(Email));
        OnPropertyChanged(nameof(PersonalDetailsNote));
    }

    // ---------- Photo ----------

    /// <summary>Take or choose a photo, then upload it straight away.</summary>
    [RelayCommand]
    private async Task PhotoOptions()
    {
        if (IsBusy) return;

        var choice = await Shell.Current.DisplayActionSheetAsync("Profile photo", "Cancel", null,
            "📷  Take a photo", "🖼  Choose from gallery");
        try
        {
            FileResult? photo = null;
            if (choice?.Contains("Take") == true)
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await Shell.Current.DisplayAlertAsync("Camera", "This device can't take photos. Choose one from your gallery instead.", "OK");
                    return;
                }
                photo = await MediaPicker.Default.CapturePhotoAsync();
            }
            else if (choice?.Contains("gallery") == true)
            {
                photo = (await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 }))?.FirstOrDefault();
            }

            if (photo is not null) await UploadPhotoAsync(photo);
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

    private async Task UploadPhotoAsync(FileResult photo)
    {
        IsBusy = true;
        try
        {
            await using var stream = await photo.OpenReadAsync();
            var contentType = string.IsNullOrWhiteSpace(photo.ContentType) ? "image/jpeg" : photo.ContentType;
            var result = await _account.ChangePhotoAsync(stream, photo.FileName, contentType);
            if (result.IsFailure)
            {
                ShowError(result.Error!.DisplayMessage);
                return;
            }

            PhotoPath = _api.ResolveFileUrl(result.Value);
            await _auth.RefreshProfileAsync();   // My account shows the new photo too
            ShowInfo("Photo updated.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RemovePhoto()
    {
        if (IsBusy) return;

        var ok = await Shell.Current.DisplayAlertAsync("Remove photo?", "Your initials will show instead.",
                                                       "Remove", "Keep");
        if (!ok) return;

        IsBusy = true;
        try
        {
            var result = await _account.RemovePhotoAsync();
            if (result.IsFailure)
            {
                ShowError(result.Error!.DisplayMessage);
                return;
            }

            PhotoPath = null;
            await _auth.RefreshProfileAsync();
            ShowInfo("Photo removed.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ---------- Save ----------

    /// <summary>Saves the profile, then the personal details (when filled in and changed), then reloads.</summary>
    [RelayCommand]
    private async Task Save()
    {
        if (IsBusy || _session.Profile is null) return;

        // 1. Check on the phone first (the API checks again)
        var first = (FirstName ?? string.Empty).Trim();
        var last = (LastName ?? string.Empty).Trim();
        var phone = (PhoneNumber ?? string.Empty).Trim();

        if (first.Length == 0 || last.Length == 0)
        {
            ShowError("Please enter your first and last name.");
            return;
        }

        if (Country is null)
        {
            ShowError("Please choose your country.");
            return;
        }

        var personal = ReadPersonalDetails(out var personalProblem);
        if (personalProblem is not null)
        {
            ShowError(personalProblem);
            return;
        }

        IsBusy = true;
        try
        {
            // 2. Profile
            var profile = await _account.UpdateProfileAsync(
                new UpdateProfileCommand(first, last, Country.ApiCountry, phone.Length == 0 ? null : phone));
            if (profile.IsFailure)
            {
                ShowError(profile.Error!.DisplayMessage);
                return;
            }

            // 3. Personal details (only when filled in and changed)
            if (personal is not null && PersonalDetailsChanged(personal))
            {
                var saved = await _account.UpdatePersonalDetailsAsync(personal);
                if (saved.IsFailure)
                {
                    await _auth.RefreshProfileAsync();   // the profile part WAS saved
                    ShowError($"Your name and phone were saved, but not your personal details: " +
                              saved.Error!.DisplayMessage);
                    return;
                }
            }

            // 4. Reload, so every screen shows the server's copy
            await _auth.RefreshProfileAsync();
            ShowInfo("Profile saved.");
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// The personal details to save: null when they're all empty (nothing to save). When only some are filled in,
    /// <paramref name="problem"/> says what's missing.
    /// </summary>
    private UpdatePersonalDetailsCommand? ReadPersonalDetails(out string? problem)
    {
        problem = null;
        var line1 = (AddressLine1 ?? string.Empty).Trim();
        var line2 = (AddressLine2 ?? string.Empty).Trim();
        var city = (City ?? string.Empty).Trim();
        var region = (RegionOrPostcode ?? string.Empty).Trim();

        var anything = DateOfBirth is not null || line1.Length > 0 || line2.Length > 0 || city.Length > 0 ||
                       region.Length > 0;
        if (!anything) return null;

        var missing = new List<string>();
        if (DateOfBirth is null) missing.Add("date of birth");
        if (line1.Length == 0) missing.Add("house number and street");
        if (city.Length == 0) missing.Add("city or town");
        if (region.Length == 0) missing.Add("region or postcode");
        if (missing.Count > 0)
        {
            problem = $"To save your personal details, please also fill in: {string.Join(", ", missing)}.";
            return null;
        }

        return new UpdatePersonalDetailsCommand(
            DateOnly.FromDateTime(DateOfBirth!.Value),
            new HomeAddressInput(line1, line2.Length == 0 ? null : line2, city, region, Country!.ApiCountry));
    }

    /// <summary>True when the personal details differ from what the server has.</summary>
    private bool PersonalDetailsChanged(UpdatePersonalDetailsCommand details)
    {
        var profile = _session.Profile!;
        var home = profile.HomeAddress;
        return profile.DateOfBirth != details.DateOfBirth ||
               home is null ||
               home.Line1 != details.HomeAddress.Line1 ||
               (home.Line2 ?? string.Empty) != (details.HomeAddress.Line2 ?? string.Empty) ||
               home.City != details.HomeAddress.City ||
               home.RegionOrPostcode != details.HomeAddress.RegionOrPostcode ||
               home.Country != details.HomeAddress.Country;
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
