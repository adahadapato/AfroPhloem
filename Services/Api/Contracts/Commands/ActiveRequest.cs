namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of "switch a route on or off" (the route is in the URL). Same as the API's <c>ActiveRequest</c>.</summary>
public sealed record ActiveRequest(bool IsActive);
