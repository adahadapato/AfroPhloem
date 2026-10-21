using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;

namespace Phloem.ViewModels;

/// <summary>
/// My shipments (route "myshipments"): the signed-in sender's parcels from the API, newest first.
/// Tapping a card opens Shipment details.
/// </summary>
public partial class MyShipmentsViewModel : ObservableObject
{
    private readonly ShipmentService _shipments;
    private readonly SessionService _session;

    public MyShipmentsViewModel(ShipmentService shipments, SessionService session)
    {
        _shipments = shipments;
        _session = session;
    }

    public ObservableCollection<ShipmentCard> Shipments { get; } = new();

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    /// <summary>"You haven't sent a parcel yet.", an error, or empty.</summary>
    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    /// <summary>Called each time the page appears (statuses change as the parcel travels).</summary>
    public async Task LoadAsync()
    {
        if (!_session.IsLoggedIn)
        {
            Shipments.Clear();
            Message = "Log in to see your parcels.";
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _shipments.GetMineAsync();
            if (result.IsFailure)
            {
                Message = result.Error!.DisplayMessage;
                return;
            }

            Shipments.Clear();
            foreach (var s in result.Value!.Items) Shipments.Add(ShipmentCard.From(s));
            Message = Shipments.Count == 0 ? "You haven't sent a parcel yet." : string.Empty;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Refresh() => await LoadAsync();

    [RelayCommand]
    private async Task Open(ShipmentCard card) =>
        await Shell.Current.GoToAsync($"shipmentdetails?tracking={Uri.EscapeDataString(card.TrackingNumber)}");

    [RelayCommand]
    private async Task SendParcel() => await Shell.Current.GoToAsync("//home/international");
}
