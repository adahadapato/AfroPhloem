namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>"Buy again": the dishes still on sale, and the names of those that aren't. Same shape as the API's <c>BuyAgainDto</c>.</summary>
public sealed record BuyAgainDto(IReadOnlyList<BasketItemDto> Items, IReadOnlyList<string> UnavailableDishes);
