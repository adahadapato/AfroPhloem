using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;
using Phloem.Services.Api;
using Phloem.Services.Api.Contracts;
using Phloem.Services.Api.Contracts.Dtos;
using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.ViewModels;

/// <summary>One bar in the "Sales overview" chart.</summary>
public record SalesBar(string Day, double BarHeight, bool IsWeekend);

/// <summary>
/// Seller dashboard (route "dashboard"). From the API:
/// <list type="number">
///   <item><description><b>Tiles:</b> balance, total orders, orders waiting, store rating.</description></item>
///   <item><description><b>Orders waiting:</b> paid orders to accept or decline. Tapping one opens it (what the
///   customer ordered) before deciding. "No orders waiting right now." when there are none.</description></item>
///   <item><description><b>Being prepared</b> and <b>Out for delivery</b>: tap to move them along.</description></item>
///   <item><description><b>Sales overview:</b> orders per day this week.</description></item>
/// </list>
/// "My dishes" still uses the demo data until the dish editor moves to the API (Seller Part B).
/// </summary>
public partial class SellerDashboardViewModel : ObservableObject
{
    private readonly MockDataService _data;
    private readonly SessionService _session;
    private readonly SellerService _seller;
    private CancellationTokenSource? _load;

    public ObservableCollection<Dish> MyDishes { get; } = new();

    /// <summary>Mon–Sun bars for the sales chart (weekend bars show in orange).</summary>
    public ObservableCollection<SalesBar> WeeklySales { get; } = new();

    /// <summary>Paid orders waiting to be accepted or declined.</summary>
    public ObservableCollection<ShopOrderCard> WaitingOrders { get; } = new();

    /// <summary>Accepted orders in the kitchen.</summary>
    public ObservableCollection<ShopOrderCard> PreparingOrders { get; } = new();

    /// <summary>Orders on their way to the customer.</summary>
    public ObservableCollection<ShopOrderCard> OutForDeliveryOrders { get; } = new();

    /// <summary>The demo shop behind "My dishes" (until Part B).</summary>
    [ObservableProperty]
    public partial Vendor? CurrentVendor { get; set; }

    /// <summary>My shop from the API (null until loaded, or when I have none).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Greeting), nameof(BalanceText), nameof(RatingText))]
    public partial MyShopDto? Shop { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalOrdersText), nameof(WaitingCountText))]
    public partial ShopOrderStatsDto? Stats { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    /// <summary>Why the dashboard couldn't load (empty when it did).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMessage))]
    public partial string Message { get; set; } = string.Empty;

    public bool HasMessage => Message.Length > 0;

    /// <summary>"Good morning, Mama Ama's Kitchen!" etc., based on the time of day.</summary>
    public string Greeting
    {
        get
        {
            var hour = DateTime.Now.Hour;
            var part = hour < 12 ? "Good morning" : hour < 17 ? "Good afternoon" : "Good evening";
            var name = Shop?.Name ?? CurrentVendor?.Name;
            return name is null ? $"{part}!" : $"{part}, {name}!";
        }
    }

    public string BalanceText => Shop is null ? "–" : $"{OrderService.SymbolOf(Shop.Currency)}{Shop.Balance:#,0.##}";
    public string RatingText => Shop is null || Shop.RatingCount == 0 ? "New" : $"{Shop.Rating:0.0}";
    public string TotalOrdersText => Stats?.TotalOrders.ToString() ?? "–";
    public string WaitingCountText => Stats is null ? "–" : $"{Stats.NewOrders:00}";

    public string WaitingTitle => $"Orders waiting ({WaitingOrders.Count})";
    public bool HasNoWaitingOrders => WaitingOrders.Count == 0;
    public string PreparingTitle => $"Being prepared ({PreparingOrders.Count})";
    public bool HasPreparingOrders => PreparingOrders.Count > 0;
    public string OutForDeliveryTitle => $"Out for delivery ({OutForDeliveryOrders.Count})";
    public bool HasOutForDeliveryOrders => OutForDeliveryOrders.Count > 0;

    public SellerDashboardViewModel(MockDataService data, SessionService session, SellerService seller)
    {
        _data = data;
        _session = session;
        _seller = seller;
        // Demo shop for "My dishes" (until Part B): the user's own, else the first one
        var userId = _session.CurrentUser?.Id;
        CurrentVendor = _data.Vendors.FirstOrDefault(v => userId != null && v.UserId == userId)
                        ?? _data.Vendors.FirstOrDefault();
        LoadMyDishes();
    }

    /// <summary>True when the shop has no dishes yet (shows the "Add your first dish" card).</summary>
    public bool HasNoDishes => MyDishes.Count == 0;

    private void LoadMyDishes()
    {
        MyDishes.Clear();
        if (CurrentVendor is not null)
        {
            foreach (var dish in _data.Dishes.Where(d => d.VendorId == CurrentVendor.Id))
                MyDishes.Add(dish);
        }
        OnPropertyChanged(nameof(HasNoDishes));
    }

    /// <summary>Called by the page each time it appears: dishes, then the shop and its orders from the API.</summary>
    public async Task ReloadAsync()
    {
        LoadMyDishes();
        OnPropertyChanged(nameof(Greeting));
        await LoadFromApiAsync();
    }

    [RelayCommand]
    private async Task Refresh() => await LoadFromApiAsync();

    private async Task LoadFromApiAsync()
    {
        _load?.Cancel();
        var load = _load = new CancellationTokenSource();
        IsLoading = true;
        try
        {
            var shop = await _seller.GetMyShopAsync(load.Token);
            if (load.IsCancellationRequested) return;
            if (shop.IsFailure)
            {
                Message = shop.Error!.Status == 404
                    ? "You don't have a shop yet. Open one from \"Become a Seller\"."
                    : shop.Error.DisplayMessage;
                return;
            }
            Shop = shop.Value;

            var stats = _seller.GetStatsAsync(load.Token);
            var waiting = _seller.GetOrdersAsync(OrderStatus.Paid, load.Token);
            var preparing = _seller.GetOrdersAsync(OrderStatus.Preparing, load.Token);
            var onTheWay = _seller.GetOrdersAsync(OrderStatus.OutForDelivery, load.Token);
            await Task.WhenAll(stats, waiting, preparing, onTheWay);
            if (load.IsCancellationRequested) return;

            var failed = new[] { (ApiResult)stats.Result, waiting.Result, preparing.Result, onTheWay.Result }
                .FirstOrDefault(r => r.IsFailure);
            Message = failed?.Error!.DisplayMessage ?? string.Empty;

            if (stats.Result.IsSuccess)
            {
                Stats = stats.Result.Value;
                ShowSales(Stats!.WeeklySales);
            }
            Fill(WaitingOrders, waiting.Result);
            Fill(PreparingOrders, preparing.Result);
            Fill(OutForDeliveryOrders, onTheWay.Result);
            OnPropertyChanged(nameof(WaitingTitle));
            OnPropertyChanged(nameof(HasNoWaitingOrders));
            OnPropertyChanged(nameof(PreparingTitle));
            OnPropertyChanged(nameof(HasPreparingOrders));
            OnPropertyChanged(nameof(OutForDeliveryTitle));
            OnPropertyChanged(nameof(HasOutForDeliveryOrders));
        }
        catch (OperationCanceledException)
        {
            // A newer load replaced this one
        }
        catch (Exception)
        {
            Message = "We couldn't load your orders. Pull down or tap Refresh to try again.";
        }
        finally
        {
            if (_load == load) IsLoading = false;
        }
    }

    private static void Fill(ObservableCollection<ShopOrderCard> list,
                             ApiResult<PagedResult<ShopOrderSummaryDto>> result)
    {
        if (result.IsFailure) return;
        list.Clear();
        foreach (var order in result.Value!.Items)
            list.Add(new ShopOrderCard { Order = order });
    }

    /// <summary>Mon–Sun bars from the server's daily order counts (the tallest day fills the chart).</summary>
    private void ShowSales(IReadOnlyList<DailySalesDto> days)
    {
        const double tallest = 64;
        var max = Math.Max(1, days.Count == 0 ? 0 : days.Max(d => d.Orders));

        WeeklySales.Clear();
        foreach (var day in days.OrderBy(d => d.Date))
        {
            var weekend = day.Date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            WeeklySales.Add(new SalesBar(day.Date.ToString("ddd")[..1],
                                         Math.Max(8, tallest * day.Orders / max), weekend));
        }
    }

    /// <summary>Tap an order → its page: what the customer ordered, then Accept / Decline / Send out / Delivered.</summary>
    [RelayCommand]
    private async Task OpenOrder(ShopOrderCard card) =>
        await Shell.Current.GoToAsync("shoporder", new Dictionary<string, object> { ["OrderNumber"] = card.OrderNumber });

    /// <summary>"+ Add dish" → Add dish page.</summary>
    [RelayCommand]
    private async Task AddDish() => await Shell.Current.GoToAsync("dishedit");

    /// <summary>"Shop settings" → business name, email and phone.</summary>
    [RelayCommand]
    private async Task ShopSettings() => await Shell.Current.GoToAsync("shopsettings");

    /// <summary>Tap a dish → Edit dish page.</summary>
    [RelayCommand]
    private async Task EditDish(Dish dish) =>
        await Shell.Current.GoToAsync("dishedit", new Dictionary<string, object> { ["Dish"] = dish });

    [RelayCommand]
    private void IncreasePrice(Dish dish) => dish.Price += 1;

    [RelayCommand]
    private void DecreasePrice(Dish dish)
    {
        if (dish.Price > 1) dish.Price -= 1;
    }

    /// <summary>"View all →": opens Explore filtered to this vendor's dishes.</summary>
    [RelayCommand]
    private async Task ViewAllDishes()
    {
        if (CurrentVendor is null) return;
        await Shell.Current.GoToAsync(
            $"//listing?vendorId={Uri.EscapeDataString(CurrentVendor.Id)}&vendorName={Uri.EscapeDataString(CurrentVendor.Name)}");
    }
}
