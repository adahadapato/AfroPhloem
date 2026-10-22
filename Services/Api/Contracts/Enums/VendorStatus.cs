namespace Phloem.Services.Api.Contracts.Enums;

/// <summary>Where a shop is in its life. Same names as the API's <c>VendorStatus</c>.</summary>
public enum VendorStatus
{
    /// <summary>Just opened (or fixed after a rejection): waiting for an admin, hidden from buyers.</summary>
    PendingApproval = 1,

    /// <summary>Live: buyers can see it.</summary>
    Approved = 2,

    /// <summary>Hidden by an admin.</summary>
    Blocked = 3,

    /// <summary>Not approved, with a reason the seller can fix.</summary>
    Rejected = 4,

    /// <summary>The owner deleted their account.</summary>
    Closed = 5
}
