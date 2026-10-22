namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One day's orders in the seller's sales chart. Same shape as the API's <c>DailySalesDto</c>.</summary>
public sealed record DailySalesDto(DateOnly Date, int Orders);
