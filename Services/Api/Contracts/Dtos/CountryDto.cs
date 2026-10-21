using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>
/// One country from <c>GET /api/countries</c>. Same shape as the API's <c>CountryDto</c>.
/// Send <see cref="Country"/> back to the API; show the rest.
/// </summary>
public sealed record CountryDto(
    Country Country,
    string Name,
    string IsoCode,
    string Flag,
    Currency Currency,
    string CurrencySymbol,
    bool CanSendParcels,
    IReadOnlyList<Country> SendsParcelsTo);
