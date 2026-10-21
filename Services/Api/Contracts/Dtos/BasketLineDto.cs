namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One dish in a checked basket, at today's price. Same shape as the API's <c>BasketLineDto</c>.</summary>
public sealed record BasketLineDto(
    Guid DishId,
    string DishName,
    string? ImageUrl,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    bool IsAvailable);
