using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfroPhloem.Models;
using AfroPhloem.Services;
using AfroPhloem.Views;

namespace AfroPhloem.ViewModels;
/// <summary>
/// View model for the home page, managing the display of 
/// countries, categories, featured dishes, and top sellers.
/// </summary>
public partial class HomeViewModel : ObservableObject
{
    private readonly MockDataService _data;
    private readonly CartService _cart;

    public ObservableCollection<CountryOption> Countries { get; } = new(CountryOption.All);
    public ObservableCollection<string> Categories { get; } = new() { "Popular", "Local Meals", "Snacks", "Drinks" };
    public ObservableCollection<Dish> FeaturedDishes { get; } = new();
    public ObservableCollection<Vendor> TopSellers { get; } = new();

    [ObservableProperty]
    private CountryOption selectedCountry;

    [ObservableProperty]
    private string selectedCategory = "Popular";

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private int cartCount;


/// <summary>
/// Initializes a new instance of the HomeViewModel class.
/// </summary>
/// <param name="data">The service that provides mock data.</param>
/// <param name="cart">The service that manages the shopping cart.</param>
    public HomeViewModel(MockDataService data, CartService cart)
    {
        _data = data;
        _cart = cart;
        selectedCountry = _cart.SelectedCountry;
        _cart.Items.CollectionChanged += (_, _) => RefreshCartCount();
        RefreshDishes();
        RefreshCartCount();
        foreach (var v in _data.TopSellers) TopSellers.Add(v);
    }

    private void RefreshCartCount() => CartCount = _cart.Items.Sum(i => i.Quantity);

    partial void OnSelectedCountryChanged(CountryOption value)
    {
        _cart.SelectedCountry = value;
        RefreshDishes();
    }

    partial void OnSelectedCategoryChanged(string value) => RefreshDishes();

    partial void OnSearchTextChanged(string value) => RefreshDishes();

    private void RefreshDishes()
    {
        FeaturedDishes.Clear();
        foreach (var dish in _data.GetDishes(SelectedCountry?.Name, SelectedCategory, SearchText))
            FeaturedDishes.Add(dish);
    }

    [RelayCommand]
    private void AddToCart(Dish dish)
    {
        _cart.AddDish(dish);
        RefreshCartCount();
    }

    [RelayCommand]
    private async Task GoToCheckout()
    {
        if (_cart.Items.Count == 0) return;
        await Shell.Current.GoToAsync(nameof(CheckoutPage));
    }

    [RelayCommand]
    private async Task GoToBuy() => await Shell.Current.GoToAsync("//listing");

    [RelayCommand]
    private async Task GoToVendors() => await Shell.Current.GoToAsync("//vendors");

    [RelayCommand]
    private async Task GoToSell() => await Shell.Current.GoToAsync("//sellersetup");

    [RelayCommand]
    private async Task ViewShop(Vendor vendor) =>
        await Shell.Current.GoToAsync(
            $"//listing?vendorId={Uri.EscapeDataString(vendor.Id)}&vendorName={Uri.EscapeDataString(vendor.Name)}");
}