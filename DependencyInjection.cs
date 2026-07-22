using AfroPhloem.Services;
using AfroPhloem.ViewModels;
using AfroPhloem.Views;

namespace AfroPhloem;

/// <summary>
/// class DependencyInjection is a static class that provides a method to 
/// register services, view models, and pages for dependency injection in the application.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// configures the dependency injection container 
    /// by registering services, view models, and pages.
    /// </summary>
    /// <param name="services"></param>
    public static void RegisterServices(IServiceCollection services)
    {
        // ---- Services (singletons so mock "database" is shared across pages) ----
        services.AddSingleton<MockDataService>();
        services.AddSingleton<CartService>();
        services.AddSingleton<SessionService>();

        // ---- ViewModels ----
        services.AddTransient<HomeViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<SellerSetupViewModel>();
        services.AddTransient<FoodListingViewModel>();
        services.AddTransient<SellerDashboardViewModel>();
        services.AddTransient<InternationalDeliveryViewModel>();
        services.AddTransient<CheckoutViewModel>();
        services.AddTransient<VendorsListViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<AdminViewModel>();

        // ---- Pages ----
        services.AddTransient<AboutPage>();
        services.AddTransient<HomePage>();
        services.AddTransient<RegisterPage>();
        services.AddTransient<SellerSetupPage>();
        services.AddTransient<FoodListingPage>();
        services.AddTransient<SellerDashboardPage>();
        services.AddTransient<InternationalDeliveryPage>();
        services.AddTransient<CheckoutPage>();
        services.AddTransient<VendorsListPage>();
        services.AddTransient<LoginPage>();
        services.AddTransient<AdminPage>();
    }
}
