using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfroPhloem.Models;
using AfroPhloem.Services;
using AfroPhloem.Views;

namespace AfroPhloem.ViewModels;

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

    public VendorsListViewModel(MockDataService data, CartService cart)
    {
        _data = data;
        _cart = cart;
        selectedCountry = _cart.SelectedCountry;
        RefreshVendors();
    }

    partial void OnSelectedCountryChanged(CountryOption value)
    {
        _cart.SelectedCountry = value;
        RefreshVendors();
    }

    partial void OnSearchTextChanged(string value) => RefreshVendors();

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

    [RelayCommand]
    private async Task ViewShop(Vendor vendor)
    {
        await Shell.Current.GoToAsync(
            $"//listing?vendorId={Uri.EscapeDataString(vendor.Id)}&vendorName={Uri.EscapeDataString(vendor.Name)}");
    }
}