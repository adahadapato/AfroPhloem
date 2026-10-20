using System.Net.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Phloem.Settings;

namespace Phloem.Services.Api;

/// <summary>
/// Registers everything needed to call the API:
/// <list type="number">
///   <item><description><b>Settings:</b> binds <see cref="ApiSettings"/>.</description></item>
///   <item><description><b>Tokens:</b> <see cref="TokenStore"/> (one for the whole app).</description></item>
///   <item><description><b>HttpClients:</b> the normal one (with <see cref="AuthTokenHandler"/>) and the one used
///   only to renew tokens.</description></item>
///   <item><description><b>Client:</b> <see cref="ApiClient"/>.</description></item>
/// </list>
/// </summary>
public static class ApiSetup
{
    /// <summary>Hosts that may use the local development certificate (Debug builds only).</summary>
    private static readonly string[] DevelopmentHosts = ["localhost", "127.0.0.1", "10.0.2.2"];

    /// <summary>Adds the API client.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The app's settings.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddPhloemApi(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Settings
        services.Configure<ApiSettings>(configuration.GetSection(ApiSettings.SectionName));

        // 2. Tokens
        services.AddSingleton<TokenStore>();
        services.AddTransient<AuthTokenHandler>();

        // 3. HttpClients
        services.AddHttpClient(ApiClient.ClientName, Configure)
                .ConfigurePrimaryHttpMessageHandler(CreatePrimaryHandler)
                .AddHttpMessageHandler<AuthTokenHandler>();

        services.AddHttpClient(ApiClient.RefreshClientName, Configure)
                .ConfigurePrimaryHttpMessageHandler(CreatePrimaryHandler);

        // 4. Client
        services.AddSingleton<ApiClient>();

        return services;
    }

    /// <summary>Address and timeout from the settings.</summary>
    private static void Configure(IServiceProvider services, HttpClient client)
    {
        var settings = services.GetRequiredService<IOptions<ApiSettings>>().Value;
        client.BaseAddress = settings.ResolveBaseAddress();
        client.Timeout = TimeSpan.FromSeconds(Math.Max(5, settings.TimeoutSeconds));
    }

    /// <summary>
    /// The handler that actually sends requests. In DEBUG builds only, and only when the setting allows it, the local
    /// development certificate is accepted for localhost / the emulator's 10.0.2.2. Release builds always use the
    /// normal certificate checks.
    /// </summary>
    private static HttpMessageHandler CreatePrimaryHandler(IServiceProvider services)
    {
        var handler = new HttpClientHandler();
#if DEBUG
        var settings = services.GetRequiredService<IOptions<ApiSettings>>().Value;
        if (settings.TrustDevelopmentCertificate)
        {
            handler.ServerCertificateCustomValidationCallback = (request, _, _, errors) =>
                errors == SslPolicyErrors.None ||
                DevelopmentHosts.Contains(request.RequestUri?.Host, StringComparer.OrdinalIgnoreCase);
        }
#endif
        return handler;
    }
}
