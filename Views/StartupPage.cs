using Microsoft.Extensions.Options;
using Phloem.Services;
using Phloem.Settings;

namespace Phloem.Views;

/// <summary>
/// Shown for a moment when the app opens: loads what every screen needs from the API (the countries), restores the
/// user's sign-in, then opens the app (<see cref="AppShell"/>).
/// </summary>
/// <remarks>
/// On a FIRST start without internet there are no countries yet, so it says so and offers "Try again".
/// After that, the countries kept on the phone are used and the app opens offline too.
/// </remarks>
public sealed class StartupPage : ContentPage
{
    private readonly CountryService _countries;
    private readonly AuthService _auth;
    private readonly ApiSettings _api;
    private readonly ActivityIndicator _spinner = new() { IsRunning = true, Color = Colors.White };
    private readonly Label _message = new()
    {
        Text = "Loading Phloem…", TextColor = Colors.White, FontSize = 15,
        HorizontalTextAlignment = TextAlignment.Center
    };
    private readonly Label _details = new()
    {
        TextColor = Colors.White, Opacity = 0.6, FontSize = 12, HorizontalTextAlignment = TextAlignment.Center,
        IsVisible = false
    };
    private readonly Button _retry = new() { Text = "Try again", IsVisible = false };
    private bool _started;

    /// <summary>Builds the page (no XAML: a logo, a spinner, a message and a retry button).</summary>
    public StartupPage(CountryService countries, AuthService auth, IOptions<ApiSettings> api)
    {
        _countries = countries;
        _auth = auth;
        _api = api.Value;

        BackgroundColor = Application.Current?.Resources.TryGetValue("BrandDark", out var brand) == true &&
                          brand is Color color ? color : Color.FromArgb("#2B3A25");
        Shell.SetNavBarIsVisible(this, false);

        _retry.Clicked += async (_, _) => await StartAsync();

        Content = new VerticalStackLayout
        {
            Spacing = 18,
            Padding = 32,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new Image { Source = "header_logo.png", WidthRequest = 72, HeightRequest = 72 },
                new Label
                {
                    Text = "Phloem", TextColor = Colors.White, FontSize = 28, FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                _spinner,
                _message,
                _details,
                _retry
            }
        };
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_started) return;
        _started = true;
        await StartAsync();
    }

    /// <summary>Loads the countries and the sign-in, then opens the app.</summary>
    private async Task StartAsync()
    {
        _retry.IsVisible = false;
        _details.IsVisible = false;
        _spinner.IsRunning = true;
        _message.Text = "Loading Phloem…";

        if (!await _countries.LoadAsync())
        {
            _spinner.IsRunning = false;
            _message.Text = _countries.LastError?.DisplayMessage
                            ?? "We couldn't reach Phloem. Please check your internet connection.";

            // Small print that says WHY (e.g. "Network.Unreachable" or "HTTP 500") and which address was called,
            // so a wrong address or a server problem can be told apart from no internet
            var error = _countries.LastError;
            var reason = error is null ? "No countries" : error.Status > 0 ? $"HTTP {error.Status} · {error.Code}" : error.Code;
            _details.Text = $"{reason}\n{ServerAddress()}";
            _details.IsVisible = true;
            _retry.IsVisible = true;
            return;
        }

        try
        {
            await _auth.RestoreSessionAsync();   // signed back in if "Remember me" was ticked
        }
        catch (Exception)
        {
            // Never block the app because of the sign-in: the user can sign in by hand
        }

        if (Window is not null) Window.Page = new AppShell();
    }

    /// <summary>The API address this device calls (shown when start-up fails).</summary>
    private string ServerAddress()
    {
        try { return _api.ResolveBaseAddress().ToString(); }
        catch (InvalidOperationException ex) { return ex.Message; }
    }
}
