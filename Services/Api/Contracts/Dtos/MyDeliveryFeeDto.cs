using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>My delivery fee and the range I may choose from (<c>GET /api/shops/mine/delivery-fee</c>). Same shape as
/// the API's <c>MyDeliveryFeeDto</c>.</summary>
public sealed record MyDeliveryFeeDto(
    decimal CurrentFee,
    bool UsesCountryMinimum,
    decimal MinAllowed,
    decimal MaxAllowed,
    Currency Currency);
