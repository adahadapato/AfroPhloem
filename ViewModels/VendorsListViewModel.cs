using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;
using Phloem.Services.Api;
using Phloem.Services.Api.Contracts.Enums;
using Phloem.Views;

namespace Phloem.ViewModels;

/// <summary>
/// One row on the "Meet local vendors" list: the vendor plus what the card shows.
/// </summary>
public class VendorListItem
{
    public required Vendor Vendor { get; init; }

    /// <summary>The shop's cover photo (from the API), or the app's default picture.</summary>
    public string CoverImage { get; init; } = Vendor.DefaultCoverPhoto;

    /// <summary>Short line under the name, e.g. "Ghanaian classics".</summary>
    public string Tagline { get; init; } = string.Empty;

    public int DishCount { get; init; }
}

/// <summary>
/// Manages the list of vendors, supports filtering by country and search text, and provides navigation to vendor
/// details.
/// </summary>
/// <remarks>Shops come from the Phloem API (<see cref="ShopService"/>): approved shops only.</remarks>
public partial class VendorsListViewModel : ObservableObject
{
    private readonly ShopService _shops;
    private readonly CartService _cart;
    private CancellationTokenSource? _load;

    public ObservableCollection<CountryOption> Countries { get; } = new(CountryOption.All);

    /// <summary>The cards shown on the page.</summary>
    public ObservableCollection<VendorListItem> Vendors { get; } = new();

    /// <summary>Country used across the app (header flag, currency). Unchanged behaviour.</summary>
    [ObservableProperty]
    public partial CountryOption SelectedCountry { get; set; }

    /// <summary>NEW: which chip is selected: "All", "Ghana", "Nigeria" or "United Kingdom".</summary>
    [ObservableProperty]
    public partial string CountryFilter { get; set; } = "All";

    [ObservableProperty]
    private string searchText = string.Empty;

    /// <summary>True when there's a message to show instead of shops (nothing matches, or no connection).</summary>
    public bool IsEmpty => !string.IsNullOrEmpty(EmptyMessage);

    /// <summary>Why no shops are shown: nothing matches, or the API couldn't be reached.</summary>
    [ObservableProperty]
    public partial string EmptyMessage { get; set; } = string.Empty;

    partial void OnEmptyMessageChanged(string value) => OnPropertyChanged(nameof(IsEmpty));

    public VendorsListViewModel(ShopService shops, CartService cart)
    {
        _shops = shops;
        _cart = cart;
        SelectedCountry = _cart.SelectedCountry;
        RefreshVendors();
    }

    partial void OnSelectedCountryChanged(CountryOption value)
    {
        _cart.SelectedCountry = value;
    }

    partial void OnCountryFilterChanged(string value) => RefreshVendors();

    partial void OnSearchTextChanged(string value) => RefreshVendors();

    /// <summary>NEW: the All vendors / Ghana / Nigeria / UK chips.</summary>
    [RelayCommand]
    private void FilterCountry(string country) => CountryFilter = country;

    /// <summary>Called by the page each time it appears (new approvals, new dishes).</summary>
    public void Reload()
    {
        SelectedCountry = _cart.SelectedCountry;
        RefreshVendors();
    }

    /// <summary>Reloads the shops from the API; a newer request (another letter typed) cancels the older one.</summary>
    private async void RefreshVendors()
    {
        _load?.Cancel();
        var load = _load = new CancellationTokenSource();
        try
        {
            await Task.Delay(250, load.Token);   // wait while the user is still typing

            // The chips use the names people read ("United Kingdom"); the API wants its own name
            var country = CountryFilter == "All"
                ? (Country?)null
                : CountryOption.All.FirstOrDefault(c => c.Name == CountryFilter)?.ApiCountry;

            var result = await _shops.GetShopsAsync(country, SearchText, load.Token);
            if (load.IsCancellationRequested) return;

            Vendors.Clear();
            if (result.IsFailure)
            {
                EmptyMessage = result.Error!.DisplayMessage;
                return;
            }

            // The API lists verified shops first, then the best rated
            foreach (var vendor in result.Value!)
            {
                Vendors.Add(new VendorListItem
                {
                    Vendor = vendor,
                    DishCount = vendor.DishCount,
                    CoverImage = vendor.CoverPhotoUrl,
                    Tagline = vendor.Country switch
                    {
                        "Ghana" => "Ghanaian classics",
                        "Nigeria" => "Nigerian favourites",
                        "United Kingdom" => "Diaspora kitchen",
                        _ => "African home cooking"
                    }
                });
            }

            EmptyMessage = Vendors.Count == 0 ? "No vendors match your search yet." : string.Empty;
        }
        catch (OperationCanceledException)
        {
            // A newer search replaced this one
        }
        catch (Exception)
        {
            // Never close the app because a list couldn't load: say so instead
            EmptyMessage = ApiError.Unexpected(0).DisplayMessage;
        }
    }

    /// <summary>
    /// command to navigate to the vendor's shop page, 
    /// passing the vendor's ID and name as query parameters in the URL.
    /// </summary>
    [RelayCommand]
    private async Task ViewShop(VendorListItem item)
    {
        var vendor = item.Vendor;
        await Shell.Current.GoToAsync(
            $"//listing?vendorId={Uri.EscapeDataString(vendor.Id)}&vendorName={Uri.EscapeDataString(vendor.Name)}");
    }
}
