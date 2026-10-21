using System.Text.Json;
using Microsoft.Extensions.Logging;
using Phloem.Models;
using Phloem.Services.Api;
using Phloem.Services.Api.Contracts.Dtos;

namespace Phloem.Services;

/// <summary>
/// The countries Phloem works in, from the API (<c>GET /api/countries</c>). The app keeps no list of its own.
/// </summary>
/// <remarks>
/// <para>The last list is kept on the phone, so the app still opens without internet after its first start.</para>
/// <para>Fills <see cref="CountryOption.All"/>, which the existing screens use.</para>
/// </remarks>
public sealed class CountryService(ApiClient api, ILogger<CountryService> logger)
{
    /// <summary>Where the last list is kept on the phone.</summary>
    private const string SavedListKey = "phloem.countries";

    /// <summary>The countries, once loaded; empty before.</summary>
    public IReadOnlyList<CountryOption> Countries { get; private set; } = [];

    /// <summary>True when there is a list to show.</summary>
    public bool IsLoaded => Countries.Count > 0;

    /// <summary>Why the last <see cref="LoadAsync"/> couldn't get the list from the API; null when it could.</summary>
    public ApiError? LastError { get; private set; }

    /// <summary>
    /// Gets the list from the API (and keeps it on the phone); without internet, uses the list kept last time.
    /// </summary>
    /// <returns>True when there is a list (fresh or kept); false on a first start without internet.</returns>
    public async Task<bool> LoadAsync(CancellationToken ct = default)
    {
        var result = await api.GetAsync<List<CountryDto>>("api/countries", ct);
        LastError = result.IsSuccess ? null : result.Error;
        if (result.IsSuccess && result.Value!.Count > 0)
        {
            Use(result.Value);
            Save(result.Value);
            return true;
        }

        logger.LogInformation("Countries not loaded from the API ({Code}); using the saved list.", result.Error?.Code);
        var saved = ReadSaved();
        if (saved.Count > 0) Use(saved);
        return IsLoaded;
    }

    private void Use(IReadOnlyList<CountryDto> countries)
    {
        Countries = countries.Select(CountryOption.From).ToList();
        CountryOption.Use(Countries);
    }

    private static void Save(IReadOnlyList<CountryDto> countries)
    {
        try { Preferences.Default.Set(SavedListKey, JsonSerializer.Serialize(countries, ApiJson.Options)); }
        catch (Exception) { /* not available on this device: the list is simply loaded again next time */ }
    }

    private static List<CountryDto> ReadSaved()
    {
        try
        {
            var json = Preferences.Default.Get(SavedListKey, string.Empty);
            return string.IsNullOrEmpty(json)
                ? []
                : JsonSerializer.Deserialize<List<CountryDto>>(json, ApiJson.Options) ?? [];
        }
        catch (Exception)
        {
            return [];
        }
    }
}
