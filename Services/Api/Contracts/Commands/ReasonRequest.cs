namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of "reject a shop": why (the seller sees it). Same as the API's <c>ReasonRequest</c>.</summary>
public sealed record ReasonRequest(string Reason);
