using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One order created by <c>POST /api/orders</c> (one per shop). Same shape as the API's <c>PlacedOrderDto</c>.</summary>
public sealed record PlacedOrderDto(string OrderNumber, string ShopName, decimal Total, Currency Currency);
