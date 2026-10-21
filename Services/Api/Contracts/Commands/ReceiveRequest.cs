namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of "received at the hub" (staff; the parcel is in the URL). Same as the API's <c>ReceiveRequest</c>.</summary>
public sealed record ReceiveRequest(decimal WeighedWeightKg);
