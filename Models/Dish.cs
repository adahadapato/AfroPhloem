using CommunityToolkit.Mvvm.ComponentModel;

namespace AfroPhloem.Models;

public partial class Dish : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = "dotnet_bot.png";

    [ObservableProperty]
    public partial decimal Price { get; set; }
    public double Rating { get; set; }
    public string Category { get; set; } = "Popular";
    public string Country { get; set; } = "Ghana";
    public string VendorName { get; set; } = string.Empty;
    public string VendorId { get; set; } = string.Empty;
}
