using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Phloem.Settings;

namespace Phloem;

/// <summary>
/// class MauiProgram is the entry point for the .NET MAUI application.
/// It sets up the application builder, configures fonts, settings,
/// logging, and registers services for dependency injection.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// creates and configures the MauiApp instance for the application.
    /// </summary>
    /// <returns>The configured app.</returns>
    public static MauiApp CreateMauiApp
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Add custom .ttf files to Resources/Fonts and register them here if you
                // want branded typography, e.g.:
                // fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            }).ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                handlers.AddHandler(typeof(Shell), typeof(Phloem.Platforms.PhloemShellRenderer));
#endif
            });

        // ---------- Settings: release values, then (Debug only) your local API on top ----------
        AddEmbeddedSettings(builder.Configuration, AppSettings.Files.Default, required: true);
#if DEBUG
        AddEmbeddedSettings(builder.Configuration, AppSettings.Files.Development, required: false);
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif
        DependencyInjection.RegisterServices(builder.Services, builder.Configuration);

        return builder.Build();
    }

    /// <summary>Loads a settings file embedded in the app (see the EmbeddedResource items in Phloem.csproj).</summary>
    /// <param name="configuration">The app's configuration.</param>
    /// <param name="fileName">The embedded file's name, e.g. "appsettings.json".</param>
    /// <param name="required">Fail when it's missing (the default settings must always be there).</param>
    /// <exception cref="InvalidOperationException">A required settings file isn't embedded.</exception>
    private static void AddEmbeddedSettings(ConfigurationManager configuration, string fileName, bool required)
    {
        var stream = typeof(MauiProgram).Assembly.GetManifestResourceStream(fileName);
        if (stream is null)
        {
            if (required)
                throw new InvalidOperationException($"Settings/{fileName} is missing from the app (check Phloem.csproj).");
            return;
        }

        configuration.AddJsonStream(stream);
    }
}
