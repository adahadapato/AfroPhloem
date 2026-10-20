namespace Phloem.Services.Api.Contracts;

/// <summary>Role names the API puts in the sign-in token. Same values as the API's <c>Roles</c>.</summary>
public static class Roles
{
    /// <summary>Everything: approve shops, manage pricing, users and staff.</summary>
    public const string Admin = "Admin";

    /// <summary>Every signed-up user.</summary>
    public const string Customer = "Customer";

    /// <summary>Given when an admin approves the user's shop.</summary>
    public const string Seller = "Seller";

    /// <summary>Manages dish names and price ranges.</summary>
    public const string CatalogManager = "CatalogManager";

    /// <summary>Hub staff: receives, weighs and moves parcels.</summary>
    public const string ShipmentManager = "ShipmentManager";

    /// <summary>Helps customers.</summary>
    public const string SupportAgent = "SupportAgent";
}
