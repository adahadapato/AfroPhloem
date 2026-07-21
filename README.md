# Afro-Phloem — .NET MAUI App

A cross-platform (Windows, macOS/Catalyst desktop, iOS, Android, and tablet-responsive)
implementation of the Afro-Phloem wireframes: Home, Register (Buyer/Seller), Become a
Seller, Food Listing & Order, Seller Dashboard, International Delivery to the UK, and
Checkout.

Built with **.NET MAUI + MVVM (CommunityToolkit.Mvvm)**. Data is currently an in-memory
mock service (`Services/MockDataService.cs`, `Services/CartService.cs`) so the whole
app is click-through and demoable with zero backend — swap those two classes for real
`HttpClient` calls when your API is ready; the ViewModels don't need to change.

## Project layout

```
AfroPhloem/
  Models/            Vendor, Dish, CartItem, CountryOption, DeliveryOrder
  Services/          MockDataService (seed data), CartService (shared cart state)
  ViewModels/        One per screen, CommunityToolkit.Mvvm [ObservableProperty]/[RelayCommand]
  Views/             XAML pages, one per blueprint screen
  Converters/         IntToBoolConverter, InvertedBoolConverter
  Resources/Styles/  Colors.xaml (brand palette), Styles.xaml (buttons, cards, inputs)
  Platforms/         Android / iOS / MacCatalyst / Windows bootstrap files
  AppShell.xaml      Flyout navigation between all screens
  MauiProgram.cs     DI registration
```

## Screens implemented (maps to your PDF blueprint)

| # | Blueprint screen | File |
|---|---|---|
| 1 | Home / Buy-Sell landing page | `Views/HomePage.xaml` |
| 2 | Register (Buyer & Seller toggle) | `Views/RegisterPage.xaml` |
| 3 | Become a Seller (vendor setup) | `Views/SellerSetupPage.xaml` |
| 4 | Food Listing & Order | `Views/FoodListingPage.xaml` |
| 5 | Seller Dashboard | `Views/SellerDashboardPage.xaml` |
| 6 | Country & Location Selector | Built into the Home/Listing headers (`Picker` bound to `CountryOption`) |
| 7 | International Delivery — "Send African Food to the UK" | `Views/InternationalDeliveryPage.xaml` |
| 8 | Checkout & Payment | `Views/CheckoutPage.xaml` |

The country picker (Ghana 🇬🇭 / Nigeria 🇳🇬 / UK 🇬🇧) drives which dishes/vendors show,
matching the blueprint's "one app, multiple countries" behavior.

## Prerequisites

1. **Visual Studio 2022 (17.8+)** with the **.NET Multi-platform App UI development**
   workload, *or* VS Code + the .NET MAUI extension, on a machine with the .NET 8 SDK.
2. Install the MAUI workloads once:
   ```
   dotnet workload install maui
   ```
3. For iOS/Mac Catalyst builds you'll need a Mac (or a paired Mac via Visual Studio's
   remote iOS simulator) with Xcode installed.
4. For Android you need the Android SDK (Visual Studio installs this for you, or use
   `dotnet workload install maui-android` + Android Studio's SDK manager).

## Running it

```bash
cd AfroPhloem
dotnet restore
dotnet build -t:Run -f net8.0-windows10.0.19041.0   # Windows desktop
dotnet build -t:Run -f net8.0-maccatalyst           # Mac desktop
dotnet build -t:Run -f net8.0-android               # Android emulator/device
dotnet build -t:Run -f net8.0-ios                   # iOS simulator (Mac only)
```

Or simply open the folder in Visual Studio 2022, pick a target (Windows Machine,
Android Emulator, iOS Simulator, etc.) from the dropdown next to the Run button, and
press **F5**.

> **Note:** This project was generated file-by-file in a sandbox without .NET/NuGet
> access, so it has **not been compiled and verified** end-to-end. The code has been
> carefully hand-reviewed for correctness, but treat the first `dotnet build` as your
> real smoke test — if the workload template generates a few extra scaffolding files
> (e.g. `Platforms/Windows/Assets/*`, `app.manifest`) that aren't included here, add
> them from a fresh `dotnet new maui` project, or let Visual Studio's "Add > New Item"
> Windows template regenerate them.

## Responsive / multi-form-factor behavior

- All pages use `Grid`/`ScrollView`/`CollectionView` with `GridItemsLayout` so content
  reflows automatically between phone (narrow, 2-col dish grid), tablet, and desktop
  (wider window, `MaximumWidthRequest` keeps forms centered instead of stretching edge
  to edge).
- `App.xaml.cs` sets sensible default/minimum window sizes for desktop; MAUI's Shell
  flyout collapses into a hamburger on phones and stays visible as a side rail on wide
  screens automatically.

## Next steps you'll likely want

- Replace `MockDataService`/`CartService` with real API calls (auth, persistence).
- Add real photography for dishes/vendors (currently a placeholder brand-colored PNG).
- Wire up push notifications for order status (Seller Dashboard "Pending Orders").
- Add payment SDK integration (Stripe/Paystack/Flutterwave) behind the "Place Order"
  button in `CheckoutViewModel.PlaceOrder()`.
