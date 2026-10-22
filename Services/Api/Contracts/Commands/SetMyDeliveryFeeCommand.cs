namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Shop settings → delivery fee (<c>PUT /api/shops/mine/delivery-fee</c>), inside the country's range.
/// Same shape as the API's <c>SetMyDeliveryFeeCommand</c>.</summary>
public sealed record SetMyDeliveryFeeCommand(decimal Fee);
