using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Services;

namespace Phloem.ViewModels;

public partial class SellerSetupViewModel : ObservableObject
{
    private readonly MockDataService _data;
    private readonly SessionService _session;

    [ObservableProperty]
    public partial string BusinessName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BusinessEmail { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BusinessPhone { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LogoFileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsError { get; set; }

    public bool IsLoggedIn => _session.IsLoggedIn;

    public string LoggedInAsText => _session.IsLoggedIn
        ? $"Setting up a seller account for {_session.CurrentUser!.FullName} ({_session.CurrentUser!.Email})"
        : string.Empty;

    public SellerSetupViewModel(MockDataService data, SessionService session)
    {
        _data = data;
        _session = session;
    }

    /// <summary>
    /// Called by the page each time it appears, so the signed-in / not-signed-in
    /// notice updates after the person logs in or creates an account.
    /// </summary>
    public void RefreshSession()
    {
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(LoggedInAsText));
    }

    [RelayCommand]
    private async Task GoToLogin() => await Shell.Current.GoToAsync("//account/login");

    [RelayCommand]
    private async Task PickLogo()
    {
        try
        {
            var results = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 });
            var result = results?.FirstOrDefault();
            if (result is not null)
                LogoFileName = result.FileName;
        }
        catch
        {
            // ignore on platforms without a media picker
        }
    }

    [RelayCommand]
    private async Task CreateVendorProfile()
    {
        if (!_session.IsLoggedIn)
        {
            IsError = true;
            StatusMessage = "Please log in first - a seller account is linked to your existing Phloem account.";
            return;
        }

        if (string.IsNullOrWhiteSpace(BusinessName))
        {
            IsError = true;
            StatusMessage = "Business name is required.";
            return;
        }

        _data.RegisterVendor(BusinessName, _session.CurrentUser!, BusinessEmail, BusinessPhone);
        IsError = false;
        StatusMessage = $"{BusinessName} has been submitted and is awaiting admin approval before it goes live.";
        await Shell.Current.GoToAsync("//home");
    }
}
