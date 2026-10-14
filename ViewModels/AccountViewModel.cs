using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Services;

namespace Phloem.ViewModels;

/// <summary>
/// My Account (the Account tab). Shows the signed-in customer's details and a Log out button,
/// or Log in / Create account when nobody is signed in.
/// </summary>
public partial class AccountViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly MockDataService _data;
    private readonly OrderHistoryService _history;
    private readonly DeliveryAddressService _delivery;

    public AccountViewModel(SessionService session, MockDataService data,
                            OrderHistoryService history, DeliveryAddressService delivery)
    {
        _session = session;
        _data = data;
        _history = history;
        _delivery = delivery;
    }

    // ---------- Who is signed in ----------
    public bool IsLoggedIn => _session.IsLoggedIn && _session.CurrentUser is not null;
    public bool IsGuest => !IsLoggedIn;

    public string FullName => _session.CurrentUser?.FullName ?? string.Empty;
    public string Email => _session.CurrentUser?.Email ?? string.Empty;
    public string PhoneNumber => string.IsNullOrWhiteSpace(_session.CurrentUser?.PhoneNumber)
                                 ? "Not added" : _session.CurrentUser!.PhoneNumber;
    public string Country => _session.CurrentUser?.Country ?? string.Empty;
    public string MemberSince => _session.CurrentUser is null ? string.Empty
                                 : $"Member since {_session.CurrentUser.JoinedAt:MMMM yyyy}";

    /// <summary>"EO" for Efua Owusu.</summary>
    public string Initials
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => "?",
                1 => parts[0][..1].ToUpper(),
                _ => $"{parts[0][0]}{parts[^1][0]}".ToUpper()
            };
        }
    }

    public bool IsAdmin => _session.CurrentUser?.IsAdmin == true;

    /// <summary>True when this user has registered a seller shop.</summary>
    public bool IsSeller => _session.CurrentUser is { } u && _data.Vendors.Any(v => v.UserId == u.Id);

    public string RoleText => IsAdmin ? "Admin" : IsSeller ? "Seller" : "Customer";
    public string SellerLinkText => IsSeller ? "Seller dashboard" : "Become a seller";
    public string SellerLinkSubtitle => IsSeller ? "Your shop, orders and dishes" : "Start selling your food on Phloem";

    // ---------- Summaries ----------
    public string AddressSummary
    {
        get
        {
            var count = _session.CurrentUser?.Addresses.Count ?? 0;
            return count == 0 ? "No saved addresses yet" : $"{count} saved · {_delivery.Describe(_session.CurrentUser)}";
        }
    }

    public string OrdersSummary
    {
        get
        {
            var count = _history.Orders.Count;
            return count == 0 ? "No orders yet" : $"{count} order(s) placed";
        }
    }

    /// <summary>Called by the page each time it appears.</summary>
    public void Load()
    {
        foreach (var name in new[]
        {
            nameof(IsLoggedIn), nameof(IsGuest), nameof(FullName), nameof(Email), nameof(PhoneNumber),
            nameof(Country), nameof(MemberSince), nameof(Initials), nameof(IsAdmin), nameof(IsSeller),
            nameof(RoleText), nameof(SellerLinkText), nameof(SellerLinkSubtitle),
            nameof(AddressSummary), nameof(OrdersSummary)
        })
            OnPropertyChanged(name);
    }

    // ---------- Navigation (pages open on top of the Account tab) ----------
    [RelayCommand] private async Task Login() => await Shell.Current.GoToAsync("login");
    [RelayCommand] private async Task Register() => await Shell.Current.GoToAsync("register");
    [RelayCommand] private async Task MyOrders() => await Shell.Current.GoToAsync("//orders");
    [RelayCommand] private async Task Addresses() => await Shell.Current.GoToAsync("address");
    [RelayCommand] private async Task SellerArea() => await Shell.Current.GoToAsync(IsSeller ? "dashboard" : "sellersetup");
    [RelayCommand] private async Task AdminArea() => await Shell.Current.GoToAsync("admin");
    [RelayCommand] private async Task SendToUk() => await Shell.Current.GoToAsync("international");
    [RelayCommand] private async Task About() => await Shell.Current.GoToAsync("about");

    /// <summary>Log out: asks first, clears the session and the chosen address, then shows the guest view.</summary>
    [RelayCommand]
    private async Task Logout()
    {
        var ok = await Shell.Current.DisplayAlert("Log out?",
            $"You're signed in as {Email}.", "Log out", "Stay signed in");
        if (!ok) return;

        _session.CurrentUser = null;
        _delivery.SelectedAddress = null;
        Load();
    }
}
