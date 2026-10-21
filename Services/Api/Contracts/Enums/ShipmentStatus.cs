namespace Phloem.Services.Api.Contracts.Enums;

/// <summary>Where a parcel is on its journey. Same names as the API's <c>ShipmentStatus</c>.</summary>
public enum ShipmentStatus
{
    /// <summary>Booked; waiting to be dropped off at the hub.</summary>
    Booked = 1,

    /// <summary>Dropped off and weighed; waiting for the sender to pay the final price.</summary>
    ReceivedAtHub = 2,

    /// <summary>Paid; waiting to leave.</summary>
    Paid = 3,

    InTransit = 4,
    ArrivedInDestination = 5,
    OutForDelivery = 6,
    Delivered = 7,
    Cancelled = 8
}
