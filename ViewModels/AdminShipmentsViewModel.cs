using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;
using Phloem.Services.Api.Contracts.Enums;
using Phloem.Settings;

namespace Phloem.ViewModels;

/// <summary>A status chip on All shipments ("All", "Booked", "At the hub"…).</summary>
public partial class StatusFilter : ObservableObject
{
    public required string Name { get; init; }

    /// <summary>The status to show; null = all.</summary>
    public ShipmentStatus? Status { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}

/// <summary>
/// Admin → All shipments (route "adminshipments"): every parcel from the API, newest first, filtered by status.
/// Tapping a card opens Shipment details in staff mode (received at hub, next step).
/// </summary>
public partial class AdminShipmentsViewModel : ObservableObject
{
    private readonly ShipmentService _shipments;

    public AdminShipmentsViewModel(ShipmentService shipments, PagingFactory paging)
    {
        _shipments = shipments;
        Filters[0].IsSelected = true;
        Pager = paging.Create(PagedLists.AdminShipments, LoadAsync);
    }

    /// <summary>◀ Page x of y ▶ and "Show n" under the parcels.</summary>
    public Paging Pager { get; }

    public ObservableCollection<StatusFilter> Filters { get; } = new(
        new StatusFilter[] { new() { Name = "All" } }
            .Concat(Enum.GetValues<ShipmentStatus>().Select(s => new StatusFilter { Name = ShipmentText.Step(s), Status = s })));

    public ObservableCollection<ShipmentCard> Shipments { get; } = new();

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    private ShipmentStatus? SelectedStatus => Filters.FirstOrDefault(f => f.IsSelected)?.Status;

    /// <summary>Called each time the page appears (and by pull to refresh).</summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _shipments.GetAllAsync(SelectedStatus, Pager.Page, Pager.PageSize);
            if (result.IsFailure)
            {
                Message = result.Error!.DisplayMessage;
                return;
            }

            Pager.Update(result.Value!.TotalCount);
            Shipments.Clear();
            foreach (var s in result.Value!.Items) Shipments.Add(ShipmentCard.From(s));
            Message = Shipments.Count == 0 ? "No parcels here." : string.Empty;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Refresh() => await LoadAsync();

    [RelayCommand]
    private async Task SelectFilter(StatusFilter filter)
    {
        foreach (var f in Filters) f.IsSelected = ReferenceEquals(f, filter);
        Pager.Reset();
        await LoadAsync();
    }

    [RelayCommand]
    private async Task Open(ShipmentCard card) =>
        await Shell.Current.GoToAsync($"shipmentdetails?tracking={Uri.EscapeDataString(card.TrackingNumber)}&staff=true");
}
