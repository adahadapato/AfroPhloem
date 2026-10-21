namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>
/// Body of "set a rate" (route and speed are in the URL); amounts in the origin country's currency.
/// Same as the API's <c>ShipmentRateRequest</c>.
/// </summary>
public sealed record ShipmentRateRequest(decimal BaseCharge, decimal PricePerKg, int MinTransitDays,
                                         int MaxTransitDays);
