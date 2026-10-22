namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of <c>POST /api/shops/mine/orders/{orderNumber}/decline</c>: why (the buyer is emailed it).
/// Same shape as the API's <c>OrderEndpoints.DeclineRequest</c>.</summary>
public sealed record DeclineRequest(string Reason);
