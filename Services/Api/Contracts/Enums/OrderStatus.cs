namespace Phloem.Services.Api.Contracts.Enums;

/// <summary>Where an order is. Same names as the API's <c>OrderStatus</c> (sent as words, e.g. "AwaitingPayment").</summary>
public enum OrderStatus
{
    /// <summary>Placed, not paid yet.</summary>
    AwaitingPayment = 1,

    /// <summary>Paid; the shop hasn't started yet.</summary>
    Paid = 2,

    /// <summary>The shop accepted it and is cooking.</summary>
    Preparing = 3,

    /// <summary>On its way.</summary>
    OutForDelivery = 4,

    /// <summary>Delivered.</summary>
    Delivered = 5,

    /// <summary>Cancelled by the buyer.</summary>
    Cancelled = 6
}
