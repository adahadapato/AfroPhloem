using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>What was (or will be) charged for an order. Same shape as the API's <c>OrderPaymentDto</c>.</summary>
public sealed record OrderPaymentDto(
    string OrderNumber,
    decimal AmountToPay,
    Currency Currency,
    IReadOnlyList<string> RemovedDishes);
