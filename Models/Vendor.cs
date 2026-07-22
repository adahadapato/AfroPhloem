using CommunityToolkit.Mvvm.ComponentModel;

namespace AfroPhloem.Models;

public partial class Vendor : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>The User account that owns this vendor profile. Empty for legacy seed data.</summary>
    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty; // business name
    public string LogoUrl { get; set; } = "dotnet_bot.png";
    public double Rating { get; set; }
    public string Country { get; set; } = "Ghana";
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public decimal Balance { get; set; }
    public int DishCount { get; set; }

    // Business contact info - only filled in if different from the owning
    // user's personal email/phone. Falls back to the user's own at creation time.
    public string BusinessEmail { get; set; } = string.Empty;
    public string BusinessPhone { get; set; } = string.Empty;

    public DateTime JoinedAt { get; set; } = DateTime.Now;

    [ObservableProperty]
    public partial bool IsVerified { get; set; }

    [ObservableProperty]
    public partial bool IsApproved { get; set; } = true;

    [ObservableProperty]
    public partial bool IsBlocked { get; set; }
}