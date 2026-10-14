using CommunityToolkit.Mvvm.ComponentModel;
using Phloem.Services;

namespace Phloem.ViewModels;

/// <summary>
/// Notifications (route "notifications"). Each switch saves straight away; there's no Save button.
/// The backend will use these when it sends push notifications / emails.
/// </summary>
public partial class NotificationSettingsViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly MockDataService _data;
    private bool _loading;

    public NotificationSettingsViewModel(SessionService session, MockDataService data)
    {
        _session = session;
        _data = data;
    }

    [ObservableProperty] public partial bool OrderUpdates { get; set; }
    [ObservableProperty] public partial bool ShipmentUpdates { get; set; }
    [ObservableProperty] public partial bool Offers { get; set; }
    [ObservableProperty] public partial bool NewDishes { get; set; }
    [ObservableProperty] public partial bool ShopOrders { get; set; }

    /// <summary>"New orders for my shop" is only shown to sellers.</summary>
    public bool IsSeller => _session.CurrentUser is { } u && _data.Vendors.Any(v => v.UserId == u.Id);

    public void Load()
    {
        var user = _session.CurrentUser;
        if (user is null) return;

        _loading = true;
        OrderUpdates    = user.NotifyOrderUpdates;
        ShipmentUpdates = user.NotifyShipmentUpdates;
        Offers          = user.NotifyOffers;
        NewDishes       = user.NotifyNewDishes;
        ShopOrders      = user.NotifyShopOrders;
        _loading = false;
        OnPropertyChanged(nameof(IsSeller));
    }

    // Save each switch as soon as it's flipped
    partial void OnOrderUpdatesChanged(bool value)    { if (!_loading && _session.CurrentUser is { } u) u.NotifyOrderUpdates = value; }
    partial void OnShipmentUpdatesChanged(bool value) { if (!_loading && _session.CurrentUser is { } u) u.NotifyShipmentUpdates = value; }
    partial void OnOffersChanged(bool value)          { if (!_loading && _session.CurrentUser is { } u) u.NotifyOffers = value; }
    partial void OnNewDishesChanged(bool value)       { if (!_loading && _session.CurrentUser is { } u) u.NotifyNewDishes = value; }
    partial void OnShopOrdersChanged(bool value)      { if (!_loading && _session.CurrentUser is { } u) u.NotifyShopOrders = value; }
}
