using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace Phloem;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Window is null) return;

        // Phloem deep emerald (BrandDark)
        var green = Android.Graphics.Color.ParseColor("#043D1F");

        // Android 14 and older: colour the status bar directly
        Window.SetStatusBarColor(green);

        // Android 15+: the status bar is see-through, so paint the area behind it green
        Window.DecorView.SetBackgroundColor(green);

        // White clock / battery / signal icons on the dark green
        WindowCompat.GetInsetsController(Window, Window.DecorView).AppearanceLightStatusBars = false;
    }
}