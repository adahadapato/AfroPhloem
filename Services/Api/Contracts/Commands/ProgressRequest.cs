using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of "next step" (staff; the parcel is in the URL). Same as the API's <c>ProgressRequest</c>.</summary>
public sealed record ProgressRequest(ShipmentStatus NewStatus, string? Note);
