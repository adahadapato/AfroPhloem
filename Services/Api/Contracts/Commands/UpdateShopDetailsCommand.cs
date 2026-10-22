namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Shop settings → details (<c>PUT /api/shops/mine</c>). Blank email / phone = use my account's. Same shape
/// as the API's <c>UpdateShopDetailsCommand</c>.</summary>
public sealed record UpdateShopDetailsCommand(string ShopName, string? BusinessEmail, string? BusinessPhone);
