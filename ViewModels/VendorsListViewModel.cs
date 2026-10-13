using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;
using Phloem.Views;

namespace Phloem.ViewModels;

/// <summary>
/// One row on the "Meet local vendors" list: the vendor plus what the card shows.
/// </summary>
public class VendorListItem
{
    public required Vendor Vendor { get; init; }

    /// <summary>Photo of one of the vendor's dishes (cards have no shop photo yet).</summary>
    public string CoverImage { get; init; } = "hero_food_photo.png";

    /// <summary>Short line under the name, e.g. "Ghanaian classics".</summary>
    public string Tagline { get; init; } = string.Empty;

    public int DishCount { get; init; }
}

/// <summary>
/// Manages the list of vendors, supports filtering by country and search text, and provides navigation to vendor
/// details.
/// </summary>
public partial class VendorsListViewModel : ObservableObject
{
    private readonly MockDataService _data;
    private readonly CartService _cart;

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

    /// <summary>NEW: true when no vendor matches the search / chip.</summary>
    public bool IsEmpty => Vendors.Count == 0;

    public VendorsListViewModel(MockDataService data, CartService cart)
    {
        _data = data;
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

    private void RefreshVendors()
    {
        Vendors.Clear();

        // Only approved, non-blocked vendors are visible to buyers (same rule as Top Sellers on Home)
        var results = _data.Vendors.Where(v => v.IsApproved && !v.IsBlocked);

        if (CountryFilter != "All")
            results = results.Where(v => v.Country == CountryFilter);

        if (!string.IsNullOrWhiteSpace(SearchText))
            results = results.Where(v => v.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var vendor in results.OrderByDescending(v => v.IsVerified).ThenByDescending(v => v.Rating))
        {
            var dishes = _data.Dishes.Where(d => d.VendorId == vendor.Id).ToList();
            vendor.DishCount = dishes.Count;

            Vendors.Add(new VendorListItem
            {
                Vendor = vendor,
                DishCount = dishes.Count,
                CoverImage = dishes.FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.ImageUrl))?.ImageUrl
                             ?? "hero_food_photo.png",
                Tagline = vendor.Country switch
                {
                    "Ghana" => "Ghanaian classics",
                    "Nigeria" => "Nigerian favourites",
                    "United Kingdom" => "Diaspora kitchen",
                    _ => "African home cooking"
                }
            });
        }

        OnPropertyChanged(nameof(IsEmpty));
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
