using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of <c>POST /api/admin/shipment-routes</c>. Same shape as the API's <c>CreateShipmentRouteCommand</c>.</summary>
public sealed record CreateShipmentRouteCommand(Country FromCountry, Country ToCountry,
                                                string DropOffAddress, string DropOffHours);
