using Phloem.Services;
using Phloem.Services.Api.Contracts.Dtos;
using Phloem.Services.Api.Contracts.Enums;
using System.Diagnostics.Metrics;

namespace Phloem.Models;

/// <summary>
/// A country the app works in, as shown in the country pickers.
/// </summary>
/// <remarks>
/// <para>The list comes from the API (<c>GET /api/countries</c>) through <see cref="CountryService"/>: the app keeps
/// NO list of its own, so a new country, flag or currency needs no app update (the website uses the same endpoint).</para>
/// <para><see cref="All"/> works exactly as before for the existing screens; it is filled before the first screen
/// shows (see <c>StartupPage</c>).</para>
/// </remarks>
public class CountryOption
{
    private static IReadOnlyList<CountryOption> _loaded = [];

    /// <summary>Name shown to people, e.g. "United Kingdom".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Flag emoji, e.g. 🇬🇧.</summary>
    public string FlagEmoji { get; set; } = string.Empty;

    /// <summary>e.g. "GH₵", "₦", "£".</summary>
    public string CurrencySymbol { get; set; } = string.Empty;

    /// <summary>Two-letter ISO code, e.g. "GB" (used to guess the user's country from the phone's region).</summary>
    public string IsoCode { get; set; } = string.Empty;

    /// <summary>True when parcels can be sent FROM this country (a shipping route is switched on).</summary>
    public bool SupportsInternationalDelivery { get; set; }

    /// <summary>Where parcels can be sent from this country.</summary>
    public IReadOnlyList<Country> SendsParcelsTo { get; set; } = [];

    /// <summary>The same country as the API names it (sent as "Ghana", "Nigeria", "UnitedKingdom").</summary>
    public Country ApiCountry { get; set; }

    /// <summary>The country's currency, e.g. GHS.</summary>
    public Currency Currency { get; set; }

    public string Display => $"{FlagEmoji}  {Name}";

    /// <summary>Every country, from the API (a new list each time, the same country objects).</summary>
    public static List<CountryOption> All => [.. _loaded];

    /// <summary>The picker option for an API country.</summary>
    /// <param name="country">The API's country.</param>
    /// <exception cref="InvalidOperationException">The countries haven't been loaded.</exception>
    public static CountryOption FromApi(Country country) =>
        _loaded.FirstOrDefault(c => c.ApiCountry == country)
        ?? throw new InvalidOperationException($"Country {country} isn't loaded yet (see CountryService).");

    /// <summary>The name shown to people for an API country, e.g. UnitedKingdom → "United Kingdom".</summary>
    /// <param name="country">The API's country.</param>
    public static string NameOf(Country country) =>
        _loaded.FirstOrDefault(c => c.ApiCountry == country)?.Name ?? country.ToString();

    /// <summary>Builds a picker option from the API's country.</summary>
    public static CountryOption From(CountryDto dto) => new()
    {
        ApiCountry = dto.Country,
        Name = dto.Name,
        FlagEmoji = dto.Flag,
        IsoCode = dto.IsoCode,
        Currency = dto.Currency,
        CurrencySymbol = dto.CurrencySymbol,
        SupportsInternationalDelivery = dto.CanSendParcels,
        SendsParcelsTo = dto.SendsParcelsTo
    };

    /// <summary>Replaces the list (called by <see cref="CountryService"/> only).</summary>
    internal static void Use(IReadOnlyList<CountryOption> countries) => _loaded = countries;
}
