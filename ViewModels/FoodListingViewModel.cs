using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfroPhloem.Models;
using AfroPhloem.Services;

namespace AfroPhloem.ViewModels;

public partial class FoodListingViewModel : ObservableObject
{
    private readonly MockDataService _data;
    private readonly CartService _cart;

    public ObservableCollection<CountryOption> Countries { get; } = new(CountryOption.All);
    public ObservableCollection<string> DeliveryTimes { get; } = new() { "ASAP (30-45 min)", "Today, Evening", "Tomorrow" };
    public ObservableCollection<Dish> Dishes { get; } = new();

    [ObservableProperty]
    private CountryOption selectedCountry;

    [ObservableProperty]
    private string selectedDeliveryTime;

    [ObservableProperty]
    private int cartCount;

    [ObservableProperty]
    private decimal cartTotal;

    [ObservableProperty]
    private string? filterVendorId;

    [ObservableProperty]
    private string filterVendorName = string.Empty;

    public bool IsFilteredByVendor => !string.IsNullOrEmpty(FilterVendorId);

    public FoodListingViewModel(MockDataService data, CartService cart)
    {
        _data = data;
        _cart = cart;
        selectedCountry = _cart.SelectedCountry;
        selectedDeliveryTime = DeliveryTimes[0];
        _cart.Items.CollectionChanged += (_, _) => RefreshCartSummary();
        RefreshDishes();
        RefreshCartSummary();
    }

    /// <summary>Called from the page when it's navigated to with a vendorId query parameter.</summary>
    public void SetVendorFilter(string? vendorId, string? vendorName)
    {
        FilterVendorId = vendorId;
        FilterVendorName = vendorName ?? string.Empty;
        OnPropertyChanged(nameof(IsFilteredByVendor));
        RefreshDishes();
    }

    [RelayCommand]
    private void ClearVendorFilter() => SetVendorFilter(null, null);

    partial void OnSelectedCountryChanged(CountryOption value)
    {
        _cart.SelectedCountry = value;
        RefreshDishes();
    }

    private void RefreshDishes()
    {
        Dishes.Clear();
        var results = _data.GetDishes(SelectedCountry?.Name);

        if (IsFilteredByVendor)
            results = results.Where(d => d.VendorId == FilterVendorId);

        foreach (var dish in results)
            Dishes.Add(dish);
    }

    private void RefreshCartSummary()
    {
        CartCount = _cart.Items.Sum(i => i.Quantity);
        CartTotal = _cart.Subtotal;
    }

    [RelayCommand]
    private void AddToCart(Dish dish)
    {
        _cart.AddDish(dish);
        RefreshCartSummary();
    }

    [RelayCommand]
    private async Task GoToCheckout()
    {
        if (_cart.Items.Count == 0) return;
        await Shell.Current.GoToAsync(nameof(Views.CheckoutPage));
    }
}