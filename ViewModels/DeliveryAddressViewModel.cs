using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;

namespace Phloem.ViewModels;

/// <summary>One row in the saved-addresses list.</summary>
public partial class AddressOption : ObservableObject
{
    public required DeliveryAddress Address { get; init; }

    public string Label => string.IsNullOrWhiteSpace(Address.Label) ? "Address" : Address.Label;
    public string Details => string.IsNullOrWhiteSpace(Address.Country)
        ? Address.FullAddress
        : $"{Address.FullAddress}, {Address.Country}";
    public bool IsDefault => Address.IsDefault;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}

/// <summary>
/// Delivery address page: pick one of the signed-in user's saved addresses or add a new one.
/// </summary>
public partial class DeliveryAddressViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly CartService _cart;
    private readonly DeliveryAddressService _delivery;

    public ObservableCollection<AddressOption> Addresses { get; } = new();
    public ObservableCollection<CountryOption> Countries { get; } = new(CountryOption.All);

    public bool IsLoggedIn => _session.IsLoggedIn && _session.CurrentUser is not null;
    public bool IsGuest => !IsLoggedIn;
    public bool HasAddresses => Addresses.Count > 0;
    public string Subtitle => IsLoggedIn
        ? $"Where should we deliver, {_session.CurrentUser!.FullName.Split(' ')[0]}?"
        : "Where should we deliver your order?";

    // ---------- New address form ----------
    [ObservableProperty]
    public partial bool IsAddingNew { get; set; }

    [ObservableProperty]
    public partial string NewLabel { get; set; } = "Home";

    [ObservableProperty]
    public partial string NewAddress { get; set; } = string.Empty;

    [ObservableProperty]
    public partial CountryOption? NewCountry { get; set; }

    [ObservableProperty]
    public partial bool SaveAsDefault { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsError { get; set; }

    public DeliveryAddressViewModel(SessionService session, CartService cart, DeliveryAddressService delivery)
    {
        _session = session;
        _cart = cart;
        _delivery = delivery;
    }

    /// <summary>Called each time the page appears.</summary>
    public void Load()
    {
        Addresses.Clear();
        StatusMessage = string.Empty;

        var user = IsLoggedIn ? _session.CurrentUser : null;
        var current = _delivery.Resolve(user);

        if (user is not null)
        {
            foreach (var a in user.Addresses.OrderByDescending(a => a.IsDefault))
                Addresses.Add(new AddressOption { Address = a, IsSelected = ReferenceEquals(a, current) });
        }
        else if (current is not null)
        {
            // A guest's one-off address
            Addresses.Add(new AddressOption { Address = current, IsSelected = true });
        }

        if (Addresses.Count > 0 && !Addresses.Any(a => a.IsSelected))
            Addresses[0].IsSelected = true;

        NewCountry ??= Countries.FirstOrDefault(c => c.Name == _cart.SelectedCountry?.Name) ?? Countries.FirstOrDefault();
        SaveAsDefault = Addresses.Count == 0;
        IsAddingNew = Addresses.Count == 0;   // nothing saved yet → open the form straight away

        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(IsGuest));
        OnPropertyChanged(nameof(HasAddresses));
        OnPropertyChanged(nameof(Subtitle));
    }

    [RelayCommand]
    private void Select(AddressOption option)
    {
        foreach (var a in Addresses) a.IsSelected = ReferenceEquals(a, option);
    }

    [RelayCommand]
    private void ChooseLabel(string label) => NewLabel = label;

    [RelayCommand]
    private void ShowAddForm()
    {
        IsAddingNew = true;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private void CancelAdd()
    {
        IsAddingNew = false;
        NewAddress = string.Empty;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private void SaveNewAddress()
    {
        if (string.IsNullOrWhiteSpace(NewAddress))
        {
            IsError = true;
            StatusMessage = "Please enter the street, city and postcode.";
            return;
        }

        var address = new DeliveryAddress
        {
            Label = NewLabel,
            FullAddress = NewAddress.Trim(),
            Country = NewCountry?.Name ?? _cart.SelectedCountry?.Name ?? string.Empty,
            IsDefault = false
        };

        if (IsLoggedIn)
        {
            var user = _session.CurrentUser!;
            if (SaveAsDefault || user.Addresses.Count == 0)
            {
                foreach (var a in user.Addresses) a.IsDefault = false;
                address.IsDefault = true;
            }
            user.Addresses.Add(address);   // saved to the account
        }

        _delivery.SelectedAddress = address;
        NewAddress = string.Empty;
        IsError = false;
        Load();   // rebuild the list with the new address selected
        StatusMessage = IsLoggedIn ? "Address saved." : "Address added for this order.";
    }

    /// <summary>"Deliver to this address": remember the choice and go back.</summary>
    [RelayCommand]
    private async Task UseSelected()
    {
        var chosen = Addresses.FirstOrDefault(a => a.IsSelected);
        if (chosen is null)
        {
            IsError = true;
            StatusMessage = "Choose an address first.";
            return;
        }

        _delivery.SelectedAddress = chosen.Address;
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task GoToLogin() => await Shell.Current.GoToAsync("//login");
}
