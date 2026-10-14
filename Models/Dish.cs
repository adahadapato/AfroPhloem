using CommunityToolkit.Mvvm.ComponentModel;

namespace Phloem.Models;

public partial class Dish : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    [ObservableProperty]
    public partial decimal Price { get; set; }
    public double Rating { get; set; }
    public string Category { get; set; } = "Popular";
    public string Country { get; set; } = "Ghana";
    public string VendorName { get; set; } = string.Empty;
    public string VendorId { get; set; } = string.Empty;
    public string CatalogName { get; set; } = string.Empty;  // the list name, e.g. "Jollof Rice"
    public string Description { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}
