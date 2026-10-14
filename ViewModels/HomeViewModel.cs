using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;
using Phloem.Views;

namespace Phloem.ViewModels;
/// <summary>
/// View model for the home page, managing the display of 
/// countries, categories, featured dishes, and top sellers.
/// </summary>
public partial class HomeViewModel : ObservableObject
{
    private readonly MockDataService _data;
    private readonly CartService _cart;
    private readonly SessionService _session;

    /// <summary>
    /// Gets the collection of available countries.
    /// </summary>
    public ObservableCollection<CountryOption> Countries { get; } = new(CountryOption.All);

    /// <summary>
    /// Gets the collection of available categories.
    /// </summary>
    public ObservableCollection<string> Categories { get; } = new() { "Popular", "Local Meals", "Snacks", "Drinks" };

    /// <summary>
    /// Gets the collection of featured dishes.
    /// </summary>
    public ObservableCollection<Dish> FeaturedDishes { get; } = new();

    /// <summary>
    /// gets the collection of top sellers.
    /// </summary>
    public ObservableCollection<Vendor> TopSellers { get; } = new();

    [ObservableProperty]
    public partial CountryOption SelectedCountry { get; set; }

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
    /// <param name="session">The signed-in user (used by Sell Food).</param>
    public HomeViewModel(MockDataService data, CartService cart, SessionService session)
    {
        _data = data;
        _cart = cart;
        _session = session;
        SelectedCountry = _cart.SelectedCountry;
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

    /// <summary>Buy Food → Browse &amp; Order (Explore tab).</summary>
    [RelayCommand]
    private async Task GoToBuy() => await Shell.Current.GoToAsync("//listing");

    [RelayCommand]
    private async Task GoToVendors() => await Shell.Current.GoToAsync("//home/vendors");

    /// <summary>
    /// Sell Food:
    ///   not logged in                 → Log in
    ///   logged in and owns a shop     → Seller dashboard
    ///   logged in without a shop yet  → Become a seller
    /// </summary>
    [RelayCommand]
    private async Task GoToSell()
    {
        if (!_session.IsLoggedIn || _session.CurrentUser is null)
        {
            await Shell.Current.GoToAsync("//account/login");
            return;
        }

        var userId = _session.CurrentUser.Id;
        var isSeller = _data.Vendors.Any(v => v.UserId == userId);

        await Shell.Current.GoToAsync(isSeller ? "//home/dashboard" : "//home/sellersetup");
    }

    [RelayCommand]
    private async Task ViewShop(Vendor vendor) =>
        await Shell.Current.GoToAsync(
            $"//listing?vendorId={Uri.EscapeDataString(vendor.Id)}&vendorName={Uri.EscapeDataString(vendor.Name)}");

    [RelayCommand]
    private async Task GoToSendToUk() => await Shell.Current.GoToAsync("//home/international");
}
