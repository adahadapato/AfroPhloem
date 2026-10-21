namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of "change a route's drop-off" (the route is in the URL). Same as the API's <c>DropOffRequest</c>.</summary>
public sealed record DropOffRequest(string DropOffAddress, string DropOffHours);
