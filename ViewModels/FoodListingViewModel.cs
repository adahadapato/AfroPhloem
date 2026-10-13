using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;

namespace Phloem.ViewModels;

public partial class FoodListingViewModel : ObservableObject
{
    private readonly MockDataService _data;
    private readonly CartService _cart;

    public ObservableCollection<CountryOption> Countries { get; } = new(CountryOption.All);
    public ObservableCollection<string> DeliveryTimes { get; } = new() { "ASAP (30-45 min)", "Today, Evening", "Tomorrow" };
    public ObservableCollection<Dish> Dishes { get; } = new();

    [ObservableProperty]
    public partial CountryOption SelectedCountry { get; set; }

    [ObservableProperty]
    public partial string SelectedDeliveryTime { get; set; }

    [ObservableProperty]
    public partial int CartCount { get; set; }

    [ObservableProperty]
    public partial decimal CartTotal { get; set; }

    [ObservableProperty]
    public partial string? FilterVendorId { get; set; }

    [ObservableProperty]
    public partial string FilterVendorName { get; set; } = string.Empty;

    /// <summary>NEW: text typed in the search bar.</summary>
    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    /// <summary>NEW: selected chip: "All", "Local Meals", "Snacks" or "Drinks".</summary>
    [ObservableProperty]
    public partial string SelectedCategory { get; set; } = "All";

    /// <summary>NEW: "Rating", "PriceLow" or "PriceHigh" (set from the Filter link).</summary>
    [ObservableProperty]
    public partial string SortBy { get; set; } = "Rating";

    public bool IsFilteredByVendor => !string.IsNullOrEmpty(FilterVendorId);

    /// <summary>NEW: true when nothing matches the search / chip.</summary>
    public bool IsEmpty => Dishes.Count == 0;

    public FoodListingViewModel(MockDataService data, CartService cart)
    {
        _data = data;
        _cart = cart;
        SelectedCountry = _cart.SelectedCountry;
        SelectedDeliveryTime = DeliveryTimes[0];
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

    /// <summary>NEW: the All / Local Meals / Snacks / Drinks chips.</summary>
    [RelayCommand]
    private void FilterCategory(string category) => SelectedCategory = category;

    partial void OnSelectedCountryChanged(CountryOption value)
    {
        _cart.SelectedCountry = value;
        RefreshDishes();
    }

    partial void OnSearchTextChanged(string value) => RefreshDishes();
    partial void OnSelectedCategoryChanged(string value) => RefreshDishes();
    partial void OnSortByChanged(string value) => RefreshDishes();

    /// <summary>NEW: called by the page each time it appears (country may have changed on Home).</summary>
    public void Reload()
    {
        if (SelectedCountry != _cart.SelectedCountry)
            SelectedCountry = _cart.SelectedCountry;   // triggers RefreshDishes
        else
            RefreshDishes();
        RefreshCartSummary();
    }

    private void RefreshDishes()
    {
        Dishes.Clear();

        // GetDishes treats "Popular" as "no category filter"
        var category = SelectedCategory == "All" ? "Popular" : SelectedCategory;
        var results = _data.GetDishes(SelectedCountry?.Name, category, SearchText);

        if (IsFilteredByVendor)
            results = results.Where(d => d.VendorId == FilterVendorId);

        results = SortBy switch
        {
            "PriceLow" => results.OrderBy(d => d.Price),
            "PriceHigh" => results.OrderByDescending(d => d.Price),
            _ => results.OrderByDescending(d => d.Rating)
        };

        foreach (var dish in results)
            Dishes.Add(dish);

        OnPropertyChanged(nameof(IsEmpty));
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
