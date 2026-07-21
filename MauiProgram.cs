using AfroPhloem.Services;
using AfroPhloem.ViewModels;
using AfroPhloem.Views;
using Microsoft.Extensions.Logging;

namespace AfroPhloem;

/// <summary>
/// class MauiProgram is the entry point for the .NET MAUI application. 
/// It sets up the application builder, configures fonts, 
/// logging, and registers services for dependency injection.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// creates and configures the MauiApp instance for the application.
    /// </summary>
    /// <returns></returns>
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
        DependencyInjection.RegisterServices(builder.Services);

        return builder.Build();
	}
}
