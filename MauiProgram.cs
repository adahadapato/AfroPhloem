using AfroPhloem.Services;
using AfroPhloem.ViewModels;
using AfroPhloem.Views;
using Microsoft.Extensions.Logging;

namespace AfroPhloem;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				// Add custom .ttf files to Resources/Fonts and register them here if you
				// want branded typography, e.g.:
				// fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// ---- Services (singletons so mock "database" is shared across pages) ----
		builder.Services.AddSingleton<MockDataService>();
		builder.Services.AddSingleton<CartService>();

        // ---- ViewModels ----
       
        builder.Services.AddTransient<HomeViewModel>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<SellerSetupViewModel>();
		builder.Services.AddTransient<FoodListingViewModel>();
		builder.Services.AddTransient<SellerDashboardViewModel>();
		builder.Services.AddTransient<InternationalDeliveryViewModel>();
		builder.Services.AddTransient<CheckoutViewModel>();
        builder.Services.AddTransient<VendorsListViewModel>();

        // ---- Pages ----
        builder.Services.AddTransient<AboutPage>();
        builder.Services.AddTransient<HomePage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<SellerSetupPage>();
		builder.Services.AddTransient<FoodListingPage>();
		builder.Services.AddTransient<SellerDashboardPage>();
		builder.Services.AddTransient<InternationalDeliveryPage>();
		builder.Services.AddTransient<CheckoutPage>();
        builder.Services.AddTransient<VendorsListPage>();

        return builder.Build();
	}
}
