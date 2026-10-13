using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;

namespace Phloem.ViewModels;

public partial class AdminViewModel : ObservableObject
{
    private readonly MockDataService _data;

    public ObservableCollection<Vendor> PendingVendors { get; } = new();
    public ObservableCollection<Vendor> ActiveVendors { get; } = new();
    public ObservableCollection<DeliveryOrder> Shipments { get; } = new();

    /// <summary>NEW: everyone who has signed up (for the Users tab).</summary>
    public ObservableCollection<User> Users { get; } = new();

    [ObservableProperty]
    public partial int TotalVendors { get; set; }

    /// <summary>NEW: number of registered users (stat tile).</summary>
    [ObservableProperty]
    public partial int TotalUsers { get; set; }

    [ObservableProperty]
    public partial int PendingApprovalCount { get; set; }

    public bool HasPendingApprovals => PendingApprovalCount > 0;

    partial void OnPendingApprovalCountChanged(int value) => OnPropertyChanged(nameof(HasPendingApprovals));

    [ObservableProperty]
    public partial int BlockedCount { get; set; }

    [ObservableProperty]
    public partial int TotalShipments { get; set; }

    [ObservableProperty]
    public partial string ActiveTab { get; set; } = "Sellers";

    public AdminViewModel(MockDataService data)
    {
        _data = data;
        Refresh();
    }

    [RelayCommand]
    private void ShowSellers() => ActiveTab = "Sellers";

    /// <summary>NEW: the Users pill.</summary>
    [RelayCommand]
    private void ShowUsers() => ActiveTab = "Users";

    [RelayCommand]
    private void ShowShipments() => ActiveTab = "Shipments";

    /// <summary>Called by the page each time it appears, so new sign-ups and shops show up.</summary>
    public void Reload() => Refresh();

    private void Refresh()
    {
        PendingVendors.Clear();
        ActiveVendors.Clear();
        Shipments.Clear();
        Users.Clear();

        foreach (var vendor in _data.Vendors.Where(v => !v.IsApproved).OrderByDescending(v => v.JoinedAt))
            PendingVendors.Add(vendor);

        foreach (var vendor in _data.Vendors.Where(v => v.IsApproved).OrderByDescending(v => v.Rating))
            ActiveVendors.Add(vendor);

        foreach (var shipment in _data.Shipments.OrderByDescending(s => s.CreatedAt))
            Shipments.Add(shipment);

        // Admin accounts are not listed as customers
        foreach (var user in _data.Users.Where(u => !u.IsAdmin).OrderByDescending(u => u.JoinedAt))
            Users.Add(user);

        TotalVendors = _data.Vendors.Count;
        TotalUsers = Users.Count;
        PendingApprovalCount = PendingVendors.Count;
        BlockedCount = _data.Vendors.Count(v => v.IsBlocked);
        TotalShipments = Shipments.Count;
    }

    [RelayCommand]
    private void ApproveVendor(Vendor vendor)
    {
        vendor.IsApproved = true;
        Refresh();
    }

    [RelayCommand]
    private void RejectVendor(Vendor vendor)
    {
        _data.Vendors.Remove(vendor);
        Refresh();
    }

    [RelayCommand]
    private void ToggleBlock(Vendor vendor)
    {
        vendor.IsBlocked = !vendor.IsBlocked;
        Refresh();
    }

    [RelayCommand]
    private async Task ViewShop(Vendor vendor) =>
        await Shell.Current.GoToAsync(
            $"//listing?vendorId={Uri.EscapeDataString(vendor.Id)}&vendorName={Uri.EscapeDataString(vendor.Name)}");
}
