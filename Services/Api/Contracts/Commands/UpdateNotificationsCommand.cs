namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of <c>PUT /api/me/notifications</c>: every switch. Same shape as the API's <c>UpdateNotificationsCommand</c>.</summary>
public sealed record UpdateNotificationsCommand(
    bool OrderUpdates,
    bool ShipmentUpdates,
    bool Offers,
    bool NewDishes,
    bool ShopOrders);
