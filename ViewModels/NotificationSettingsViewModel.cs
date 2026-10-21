using CommunityToolkit.Mvvm.ComponentModel;
using Phloem.Services;
using Phloem.Services.Api.Contracts.Commands;

namespace Phloem.ViewModels;

/// <summary>
/// Notifications (route "notifications"). Each switch saves straight away to the API
/// (<c>PUT /api/me/notifications</c>, all switches at once); there's no Save button.
/// If saving fails, the switches go back to what the server has and a message says why.
/// </summary>
public partial class NotificationSettingsViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly AccountService _account;

    /// <summary>One save at a time, so quick taps reach the server in order.</summary>
    private readonly SemaphoreSlim _saving = new(1, 1);
    private bool _loading;

    public NotificationSettingsViewModel(SessionService session, AccountService account)
    {
        _session = session;
        _account = account;
    }

    [ObservableProperty] public partial bool OrderUpdates { get; set; }
    [ObservableProperty] public partial bool ShipmentUpdates { get; set; }
    [ObservableProperty] public partial bool Offers { get; set; }
    [ObservableProperty] public partial bool NewDishes { get; set; }
    [ObservableProperty] public partial bool ShopOrders { get; set; }

    /// <summary>"New orders for my shop" is only shown to people with a shop.</summary>
    public bool IsSeller => _session.HasShop;

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

    // Save as soon as a switch is flipped
    partial void OnOrderUpdatesChanged(bool value)    => SaveSoon();
    partial void OnShipmentUpdatesChanged(bool value) => SaveSoon();
    partial void OnOffersChanged(bool value)          => SaveSoon();
    partial void OnNewDishesChanged(bool value)       => SaveSoon();
    partial void OnShopOrdersChanged(bool value)      => SaveSoon();

    private async void SaveSoon()
    {
        if (_loading || _session.CurrentUser is null) return;

        await _saving.WaitAsync();
        try
        {
            var user = _session.CurrentUser;
            if (user is null) return;

            var result = await _account.UpdateNotificationsAsync(
                new UpdateNotificationsCommand(OrderUpdates, ShipmentUpdates, Offers, NewDishes, ShopOrders));

            if (result.IsSuccess)
            {
                // Keep the signed-in copy in step, so the screen opens with these values next time
                user.NotifyOrderUpdates    = OrderUpdates;
                user.NotifyShipmentUpdates = ShipmentUpdates;
                user.NotifyOffers          = Offers;
                user.NotifyNewDishes       = NewDishes;
                user.NotifyShopOrders      = ShopOrders;
                return;
            }

            Load();   // back to what the server has
            await Shell.Current.DisplayAlertAsync("Couldn't save", result.Error!.DisplayMessage, "OK");
        }
        catch (Exception)
        {
            Load();
        }
        finally
        {
            _saving.Release();
        }
    }
}
