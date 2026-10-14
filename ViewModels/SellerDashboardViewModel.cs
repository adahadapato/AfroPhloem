using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;

namespace Phloem.ViewModels;

/// <summary>One bar in the "Sales overview" chart.</summary>
public record SalesBar(string Day, double BarHeight, bool IsWeekend);

public partial class SellerDashboardViewModel : ObservableObject
{
    private readonly MockDataService _data;
    private readonly SessionService _session;

    public ObservableCollection<Dish> MyDishes { get; } = new();

    /// <summary>Mon–Sun bars for the sales chart (weekend bars show in orange).</summary>
    public ObservableCollection<SalesBar> WeeklySales { get; } = new();

    [ObservableProperty]
    public partial Vendor? CurrentVendor { get; set; }

    /// <summary>"Good morning, Mama Ama's Kitchen!" etc., based on the time of day.</summary>
    public string Greeting
    {
        get
        {
            var hour = DateTime.Now.Hour;
            var part = hour < 12 ? "Good morning" : hour < 17 ? "Good afternoon" : "Good evening";
            return CurrentVendor is null ? $"{part}!" : $"{part}, {CurrentVendor.Name}!";
        }
    }

    public SellerDashboardViewModel(MockDataService data, SessionService session)
    {
        _data = data;
        _session = session;
        // The signed-in user's own shop; falls back to the first vendor for the demo data.
        var userId = _session.CurrentUser?.Id;
        CurrentVendor = _data.Vendors.FirstOrDefault(v => userId != null && v.UserId == userId)
                        ?? _data.Vendors.FirstOrDefault();
        LoadMyDishes();
        LoadWeeklySales();
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

    /// <summary>Called by the page each time it appears (after adding / editing / deleting a dish).</summary>
    public void Reload()
    {
        LoadMyDishes();
        OnPropertyChanged(nameof(CurrentVendor));   // shop may have been renamed in Shop settings
        OnPropertyChanged(nameof(Greeting));
    }

    /// <summary>"+ Add dish" → Add dish page.</summary>
    [RelayCommand]
    private async Task AddDish() => await Shell.Current.GoToAsync("dishedit");

    /// <summary>NEW: "Shop settings" → business name, email and phone.</summary>
    [RelayCommand]
    private async Task ShopSettings() => await Shell.Current.GoToAsync("shopsettings");

    /// <summary>Tap a dish → Edit dish page.</summary>
    [RelayCommand]
    private async Task EditDish(Dish dish) =>
        await Shell.Current.GoToAsync("dishedit", new Dictionary<string, object> { ["Dish"] = dish });

    /// <summary>
    /// Mock sales for the chart. Swap the numbers for real daily order counts when you have a backend.
    /// </summary>
    private void LoadWeeklySales()
    {
        var days   = new[] { "M", "T", "W", "T", "F", "S", "S" };
        var orders = new[] { 9, 13, 11, 18, 14, 20, 17 };   // orders per day
        var max    = orders.Max();
        const double tallest = 64;                          // height of the tallest bar

        WeeklySales.Clear();
        for (var i = 0; i < days.Length; i++)
            WeeklySales.Add(new SalesBar(days[i], Math.Max(8, tallest * orders[i] / max), IsWeekend: i >= 5));
    }

    [RelayCommand]
    private void IncreasePrice(Dish dish) => dish.Price += 1;

    [RelayCommand]
    private void DecreasePrice(Dish dish)
    {
        if (dish.Price > 1) dish.Price -= 1;
    }

    [RelayCommand]
    private void AcceptNextOrder()
    {
        if (CurrentVendor is null) return;
        if (CurrentVendor.PendingOrders > 0)
        {
            CurrentVendor.PendingOrders--;
            CurrentVendor.TotalOrders++;
            OnPropertyChanged(nameof(CurrentVendor));
        }
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
