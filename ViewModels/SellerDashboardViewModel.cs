using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfroPhloem.Models;
using AfroPhloem.Services;

namespace AfroPhloem.ViewModels;

public partial class SellerDashboardViewModel : ObservableObject
{
    private readonly MockDataService _data;

    public ObservableCollection<Dish> MyDishes { get; } = new();

    [ObservableProperty]
    private Vendor? currentVendor;

    public SellerDashboardViewModel(MockDataService data)
    {
        _data = data;
        // For the mock/demo experience, treat the first vendor as "you".
        CurrentVendor = _data.Vendors.FirstOrDefault();
        if (CurrentVendor is not null)
        {
            foreach (var dish in _data.Dishes.Where(d => d.VendorId == CurrentVendor.Id))
                MyDishes.Add(dish);
        }
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
}
