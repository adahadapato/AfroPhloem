using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfroPhloem.Models;
using AfroPhloem.Services;
using AfroPhloem.Views;

namespace AfroPhloem.ViewModels;

/// <summary>
/// Manages the list of vendors, supports filtering by country and search text, and provides navigation to vendor
/// details.
/// </summary>
/// <remarks>Interacts with data and cart services to retrieve and update vendor information based on user
/// selections.</remarks>
public partial class VendorsListViewModel : ObservableObject
{
    private readonly MockDataService _data;
    private readonly CartService _cart;

    public ObservableCollection<CountryOption> Countries { get; } = new(CountryOption.All);
    public ObservableCollection<Vendor> Vendors { get; } = new();

    [ObservableProperty]
    private CountryOption selectedCountry;

    [ObservableProperty]
    private string searchText = string.Empty;

    /// <summary>
    /// creates a new instance of the VendorsListViewModel class, 
    /// initializing it with the provided data and cart services.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="cart"></param>
    public VendorsListViewModel(MockDataService data, CartService cart)
    {
        _data = data;
        _cart = cart;
        selectedCountry = _cart.SelectedCountry;
        RefreshVendors();
    }

    /// <summary>
    /// triggered when the selected country changes, updates the cart's 
    /// selected country and refreshes the vendor list accordingly.
    /// </summary>
    /// <param name="value"></param>
    partial void OnSelectedCountryChanged(CountryOption value)
    {
        _cart.SelectedCountry = value;
        RefreshVendors();
    }

    /// <summary>
    /// triggered when the search text changes, refreshes the vendor list to reflect the current search criteria.
    /// </summary>
    /// <param name="value"></param>
    partial void OnSearchTextChanged(string value) => RefreshVendors();

    /// <summary>
    /// refreshes the list of vendors based on the selected country and search text,
    /// </summary>
    private void RefreshVendors()
    {
        Vendors.Clear();

        var results = _data.Vendors.AsEnumerable();

        if (SelectedCountry is not null)
            results = results.Where(v => v.Country == SelectedCountry.Name);

        if (!string.IsNullOrWhiteSpace(SearchText))
            results = results.Where(v => v.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var vendor in results.OrderByDescending(v => v.Rating))
        {
            vendor.DishCount = _data.Dishes.Count(d => d.VendorId == vendor.Id);
            Vendors.Add(vendor);
        }
    }

    /// <summary>
    /// command to navigate to the vendor's shop page, 
    /// passing the vendor's ID and name as query parameters in the URL.
    /// </summary>
    /// <param name="vendor"></param>
    /// <returns></returns>
    [RelayCommand]
    private async Task ViewShop(Vendor vendor)
    {
        await Shell.Current.GoToAsync(
            $"//listing?vendorId={Uri.EscapeDataString(vendor.Id)}&vendorName={Uri.EscapeDataString(vendor.Name)}");
    }
}