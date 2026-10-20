using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;
using Phloem.Services.Api.Contracts;
using Phloem.Views;

namespace Phloem.ViewModels;
/// <summary>
/// View model for the home page, managing the display of
/// countries, categories, featured dishes, and top sellers.
/// </summary>
/// <remarks>Dishes and top sellers come from the Phloem API (<see cref="DishService"/>, <see cref="ShopService"/>),
/// for the country picked in the header.</remarks>
public partial class HomeViewModel : ObservableObject
{
    private readonly DishService _dishes;
    private readonly ShopService _shops;
    private readonly CartService _cart;
    private readonly SessionService _session;
    private CancellationTokenSource? _dishesLoad;
    private CancellationTokenSource? _shopsLoad;

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

    /// <summary>Shown under "Popular Dishes" when nothing could be loaded or nothing matches.</summary>
    [ObservableProperty]
    public partial string DishesMessage { get; set; } = string.Empty;

    /// <summary>True while dishes are loading.</summary>
    [ObservableProperty]
    public partial bool IsLoading { get; set; }


    /// <summary>
    /// Initializes a new instance of the HomeViewModel class.
    /// </summary>
    /// <param name="dishes">Dishes from the API.</param>
    /// <param name="shops">Shops from the API.</param>
    /// <param name="cart">The service that manages the shopping cart.</param>
    /// <param name="session">The signed-in user (used by Sell Food).</param>
    public HomeViewModel(DishService dishes, ShopService shops, CartService cart, SessionService session)
    {
        _dishes = dishes;
        _shops = shops;
        _cart = cart;
        _session = session;
        SelectedCountry = _cart.SelectedCountry;
        _cart.Items.CollectionChanged += (_, _) => RefreshCartCount();
        RefreshDishes();
        RefreshTopSellers();
        RefreshCartCount();
    }

    private void RefreshCartCount() => CartCount = _cart.Items.Sum(i => i.Quantity);

    partial void OnSelectedCountryChanged(CountryOption value)
    {
        _cart.SelectedCountry = value;
        RefreshDishes();
        RefreshTopSellers();
    }

    partial void OnSelectedCategoryChanged(string value) => RefreshDishes();

    partial void OnSearchTextChanged(string value) => RefreshDishes();

    /// <summary>Reloads the dishes; a newer request (another letter typed) cancels the older one.</summary>
    private async void RefreshDishes()
    {
        _dishesLoad?.Cancel();
        var load = _dishesLoad = new CancellationTokenSource();

        IsLoading = true;
        try
        {
            // Wait a moment while the user is still typing
            await Task.Delay(250, load.Token);

            var result = await _dishes.SearchAsync(SelectedCountry?.ApiCountry, SelectedCategory, SearchText,
                                                   shopId: null, DishSort.Rating, page: 1, load.Token);
            if (load.IsCancellationRequested) return;

            FeaturedDishes.Clear();
            if (result.IsFailure)
            {
                DishesMessage = result.Error!.DisplayMessage;
                return;
            }

            foreach (var dish in result.Value!.Items) FeaturedDishes.Add(dish);
            DishesMessage = FeaturedDishes.Count == 0 ? "No dishes here yet. Try another country or category." : string.Empty;
        }
        catch (OperationCanceledException)
        {
            // A newer search replaced this one
        }
        finally
        {
            if (_dishesLoad == load) IsLoading = false;
        }
    }

    /// <summary>Reloads "Top Sellers" for the chosen country.</summary>
    private async void RefreshTopSellers()
    {
        _shopsLoad?.Cancel();
        var load = _shopsLoad = new CancellationTokenSource();
        try
        {
            var result = await _shops.GetTopShopsAsync(SelectedCountry?.ApiCountry, ct: load.Token);
            if (load.IsCancellationRequested || result.IsFailure) return;

            TopSellers.Clear();
            foreach (var shop in result.Value!) TopSellers.Add(shop);
        }
        catch (OperationCanceledException)
        {
            // The country changed again
        }
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

        await Shell.Current.GoToAsync(_session.HasShop ? "//home/dashboard" : "//home/sellersetup");
    }

    [RelayCommand]
    private async Task ViewShop(Vendor vendor) =>
        await Shell.Current.GoToAsync(
            $"//listing?vendorId={Uri.EscapeDataString(vendor.Id)}&vendorName={Uri.EscapeDataString(vendor.Name)}");

    [RelayCommand]
    private async Task GoToSendToUk() => await Shell.Current.GoToAsync("//home/international");
}
