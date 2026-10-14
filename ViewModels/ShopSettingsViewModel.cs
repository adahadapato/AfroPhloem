using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;

namespace Phloem.ViewModels;

/// <summary>
/// Shop settings (route "shopsettings"), opened from the Seller dashboard.
/// Business details only — personal details stay on My account → Edit profile.
/// </summary>
public partial class ShopSettingsViewModel : ObservableObject
{
    private readonly MockDataService _data;
    private readonly SessionService _session;
    private Vendor? _vendor;

    public ShopSettingsViewModel(MockDataService data, SessionService session)
    {
        _data = data;
        _session = session;
    }

    [ObservableProperty] public partial string BusinessName { get; set; } = string.Empty;
    [ObservableProperty] public partial string BusinessEmail { get; set; } = string.Empty;
    [ObservableProperty] public partial string BusinessPhone { get; set; } = string.Empty;

    [ObservableProperty] public partial string StatusMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsError { get; set; }

    public bool HasShop => _vendor is not null;
    public string ShopCountry => _vendor?.Country ?? string.Empty;
    public string ShopSince => _vendor is null ? string.Empty : $"On Phloem since {_vendor.JoinedAt:MMMM yyyy}";

    /// <summary>Approved / waiting / blocked line under the title.</summary>
    public string ApprovalText => _vendor switch
    {
        null => string.Empty,
        { IsBlocked: true }   => "Blocked: buyers can't see your shop. Contact Phloem support.",
        { IsApproved: false } => "Waiting for admin approval: buyers can't see your shop yet.",
        _                     => "Approved: buyers can see your shop and dishes."
    };

    public bool IsLive => _vendor is { IsApproved: true, IsBlocked: false };
    public bool IsNotLive => !IsLive;

    public void Load()
    {
        var userId = _session.CurrentUser?.Id;
        _vendor = _data.Vendors.FirstOrDefault(v => userId != null && v.UserId == userId)
                  ?? _data.Vendors.FirstOrDefault();   // demo fallback, same as the dashboard

        BusinessName  = _vendor?.Name ?? string.Empty;
        BusinessEmail = _vendor?.BusinessEmail ?? string.Empty;
        BusinessPhone = _vendor?.BusinessPhone ?? string.Empty;
        StatusMessage = string.Empty;
        IsError = false;

        foreach (var name in new[] { nameof(HasShop), nameof(ShopCountry), nameof(ShopSince),
                                     nameof(ApprovalText), nameof(IsLive), nameof(IsNotLive) })
            OnPropertyChanged(name);
    }

    [RelayCommand]
    private async Task Save()
    {
        if (_vendor is null) return;

        var name  = (BusinessName ?? string.Empty).Trim();
        var email = (BusinessEmail ?? string.Empty).Trim();
        var phone = (BusinessPhone ?? string.Empty).Trim();

        string? problem =
            name.Length < 2 ? "Please enter your shop name." :
            _data.Vendors.Any(v => v != _vendor && string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase))
                ? "Another shop already uses that name. Please choose a different one." :
            email.Length > 0 && (!email.Contains('@') || !email.Contains('.'))
                ? "That business email doesn't look right." :
            phone.Length > 0 && phone.Count(char.IsDigit) < 7
                ? "That business phone number doesn't look right." :
            null;

        if (problem is not null)
        {
            IsError = true;
            StatusMessage = problem;
            return;
        }

        // Renaming the shop also renames it on every dish card
        if (_vendor.Name != name)
            foreach (var dish in _data.Dishes.Where(d => d.VendorId == _vendor.Id))
                dish.VendorName = name;

        // Blank email / phone fall back to the personal ones (same rule as Seller setup)
        var owner = _session.CurrentUser;
        _vendor.Name          = name;
        _vendor.BusinessEmail = email.Length > 0 ? email : owner?.Email ?? string.Empty;
        _vendor.BusinessPhone = phone.Length > 0 ? phone : owner?.PhoneNumber ?? string.Empty;

        IsError = false;
        StatusMessage = string.Empty;
        await Shell.Current.GoToAsync("..");
    }

    /// <summary>"Edit your personal details" link → My account → Edit profile.</summary>
    [RelayCommand]
    private async Task EditPersonal() => await Shell.Current.GoToAsync("//account/editprofile");
}
