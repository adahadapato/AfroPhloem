namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One dish in the basket: only its Id and quantity (every price comes from the server).</summary>
public sealed record BasketItemDto(Guid DishId, int Quantity);
