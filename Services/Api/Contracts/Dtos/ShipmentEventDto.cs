using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One step of a parcel's timeline. Same shape as the API's <c>ShipmentEventDto</c>.</summary>
public sealed record ShipmentEventDto(ShipmentStatus Status, DateTime OccurredAt, string? Note);
