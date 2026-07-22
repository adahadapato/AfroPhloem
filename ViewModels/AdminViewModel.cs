using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfroPhloem.Models;
using AfroPhloem.Services;

namespace AfroPhloem.ViewModels;

public partial class AdminViewModel : ObservableObject
{
    private readonly MockDataService _data;

    public ObservableCollection<Vendor> PendingVendors { get; } = new();
    public ObservableCollection<Vendor> ActiveVendors { get; } = new();
    public ObservableCollection<DeliveryOrder> Shipments { get; } = new();

    [ObservableProperty]
    public partial int TotalVendors { get; set; }

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

    [RelayCommand]
    private void ShowShipments() => ActiveTab = "Shipments";

    private void Refresh()
    {
        PendingVendors.Clear();
        ActiveVendors.Clear();
        Shipments.Clear();

        foreach (var vendor in _data.Vendors.Where(v => !v.IsApproved).OrderByDescending(v => v.JoinedAt))
            PendingVendors.Add(vendor);

        foreach (var vendor in _data.Vendors.Where(v => v.IsApproved).OrderByDescending(v => v.Rating))
            ActiveVendors.Add(vendor);

        foreach (var shipment in _data.Shipments.OrderByDescending(s => s.CreatedAt))
            Shipments.Add(shipment);

        TotalVendors = _data.Vendors.Count;
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