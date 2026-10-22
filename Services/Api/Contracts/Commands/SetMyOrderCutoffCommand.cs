namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Shop settings → "Orders for today close at" (<c>PUT /api/shops/mine/order-cutoff</c>), the shop's local
/// time. Same shape as the API's <c>SetMyOrderCutoffCommand</c>.</summary>
public sealed record SetMyOrderCutoffCommand(TimeOnly Cutoff);
