namespace Phloem.Settings;

/// <summary>
/// The <c>"Api"</c> section of the app's settings: where the Phloem API is and how to call it.
/// Keys are listed in <see cref="AppSettings.Api"/>.
/// </summary>
/// <remarks>
/// <para><b>Release builds</b> read <c>Settings/appsettings.json</c> (the live API).</para>
/// <para><b>Debug builds</b> also read <c>Settings/appsettings.Development.json</c> on top: your local API
/// (<c>https://localhost:7103</c>, or <c>https://10.0.2.2:7103</c> on the Android emulator).</para>
/// <para><b>Phone plugged in (not the emulator)?</b> It can't reach "localhost" on your PC: put your PC's network
/// address in <c>BaseUrl</c> (e.g. <c>https://192.168.1.20:7103</c>) and start the API with that address.</para>
/// </remarks>
public sealed class ApiSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = AppSettings.Api.Section;

    /// <summary>The API's address, without a trailing slash.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Address used on the Android emulator only; empty = <see cref="BaseUrl"/>.</summary>
    public string AndroidEmulatorBaseUrl { get; set; } = string.Empty;

    /// <summary>How long one request may take, in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Debug only: accept the local development certificate.</summary>
    public bool TrustDevelopmentCertificate { get; set; }

    /// <summary>The address to use on THIS device, ending with "/" so relative paths ("api/auth/login") append.</summary>
    /// <returns>The API's base address.</returns>
    /// <exception cref="InvalidOperationException">No address is set.</exception>
    public Uri ResolveBaseAddress()
    {
        var onAndroidEmulator = DeviceInfo.Platform == DevicePlatform.Android &&
                                DeviceInfo.DeviceType == DeviceType.Virtual;

        var url = onAndroidEmulator && !string.IsNullOrWhiteSpace(AndroidEmulatorBaseUrl)
            ? AndroidEmulatorBaseUrl
            : BaseUrl;

        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException($"{AppSettings.Api.BaseUrl} is empty in Settings/appsettings.json.");

        return new Uri(url.TrimEnd('/') + "/");
    }
}
