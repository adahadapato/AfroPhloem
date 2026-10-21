using Phloem.Services.Api.Contracts.Dtos;
using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Queries;

/// <summary>Body of <c>POST /api/orders/basket/check</c>. Same shape as the API's <c>CheckBasketQuery</c>.</summary>
public sealed record CheckBasketQuery(IReadOnlyList<BasketItemDto> Items, DeliveryWindow DeliveryWindow);
