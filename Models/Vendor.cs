using CommunityToolkit.Mvvm.ComponentModel;

namespace Phloem.Models;

public partial class Vendor : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>The User account that owns this vendor profile. Empty for legacy seed data.</summary>
    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty; // business name
    /// <summary>Picture shown when a shop has no logo yet (bundled with the app).</summary>
    public const string DefaultLogo = "header_logo.png";

    /// <summary>Picture shown when a shop has no cover photo yet (bundled with the app).</summary>
    public const string DefaultCoverPhoto = "hero_food_photo.png";

    /// <summary>Round picture on shop cards: the shop's logo from the API, or <see cref="DefaultLogo"/>.</summary>
    public string LogoUrl { get; set; } = DefaultLogo;

    /// <summary>Banner on shop cards and the shop page: from the API, or <see cref="DefaultCoverPhoto"/>.</summary>
    public string CoverPhotoUrl { get; set; } = DefaultCoverPhoto;

    public double Rating { get; set; }

    /// <summary>How many ratings <see cref="Rating"/> is the average of.</summary>
    public int RatingCount { get; set; }
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