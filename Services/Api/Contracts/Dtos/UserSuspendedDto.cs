namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>What suspending someone did. Same shape as the API's <c>UserSuspendedDto</c>.</summary>
public sealed record UserSuspendedDto(bool ShopSuspended, int CancelledShopOrders, int OrdersToRefund);
