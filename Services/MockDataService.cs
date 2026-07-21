using AfroPhloem.Models;

namespace AfroPhloem.Services;

/// <summary>
/// In-memory data source standing in for a real API. Swap the methods below
/// for HttpClient calls to your backend when one is ready — the ViewModels
/// only depend on this class's public surface, so nothing else needs to change.
/// </summary>
public class MockDataService
{
    public List<Vendor> Vendors { get; } = new()
    {
        new Vendor { Name = "Mama Ama's Kitchen", Rating = 4.8, Country = "Ghana",  TotalOrders = 120, PendingOrders = 5, Balance = 3400m, IsVerified = true },
        new Vendor { Name = "Lagos Flavours",      Rating = 4.6, Country = "Nigeria", TotalOrders = 86,  PendingOrders = 2, Balance = 1980m, IsVerified = true },
        new Vendor { Name = "Diaspora Delights",   Rating = 4.9, Country = "United Kingdom", TotalOrders = 42, PendingOrders = 1, Balance = 950m, IsVerified = false },
        new Vendor { Name = "Food Affairs Ltd",    Rating = 4.7, Country = "Ghana",  TotalOrders = 64,  PendingOrders = 3, Balance = 2150m, IsVerified = true },
    };

    public List<Dish> Dishes { get; }

    public MockDataService()
    {
        Dishes = new List<Dish>
        {
            new() { Name = "Jollof Rice",      Price = 25.00m, Rating = 4.8, Category = "Local Meals", Country = "Ghana",   VendorName = "Mama Ama's Kitchen", VendorId = Vendors[0].Id },
            new() { Name = "Waakye",           Price = 20.00m, Rating = 4.5, Category = "Local Meals", Country = "Ghana",   VendorName = "Mama Ama's Kitchen", VendorId = Vendors[0].Id },
            new() { Name = "Egusi Soup",       Price = 28.00m, Rating = 4.7, Category = "Local Meals", Country = "Nigeria", VendorName = "Lagos Flavours",      VendorId = Vendors[1].Id },
            new() { Name = "Suya Skewers",     Price = 15.00m, Rating = 4.9, Category = "Snacks",      Country = "Nigeria", VendorName = "Lagos Flavours",      VendorId = Vendors[1].Id },
            new() { Name = "Chin Chin",        Price = 8.00m,  Rating = 4.4, Category = "Snacks",      Country = "Nigeria", VendorName = "Lagos Flavours",      VendorId = Vendors[1].Id },
            new() { Name = "Zobo Drink",       Price = 6.00m,  Rating = 4.3, Category = "Drinks",      Country = "Nigeria", VendorName = "Lagos Flavours",      VendorId = Vendors[1].Id },
            new() { Name = "Kelewele",         Price = 10.00m, Rating = 4.6, Category = "Snacks",      Country = "Ghana",   VendorName = "Mama Ama's Kitchen", VendorId = Vendors[0].Id },
            new() { Name = "Sobolo",           Price = 6.00m,  Rating = 4.5, Category = "Drinks",      Country = "Ghana",   VendorName = "Mama Ama's Kitchen", VendorId = Vendors[0].Id },
            new() { Name = "Pounded Yam & Egusi", Price = 30.00m, Rating = 4.9, Category = "Local Meals", Country = "United Kingdom", VendorName = "Diaspora Delights", VendorId = Vendors[2].Id },
            new() { Name = "Banku & Tilapia", Price = 27.00m, Rating = 4.7, Category = "Local Meals", Country = "Ghana", VendorName = "Food Affairs Ltd", VendorId = Vendors[3].Id },
            new() { Name = "Meat Pie",         Price = 9.00m,  Rating = 4.6, Category = "Snacks",      Country = "Ghana",   VendorName = "Food Affairs Ltd",   VendorId = Vendors[3].Id },
        };
    }

    public IEnumerable<Dish> GetDishes(string? country = null, string? category = null, string? searchText = null)
    {
        var query = Dishes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(country) && country != "All")
            query = query.Where(d => d.Country == country);

        if (!string.IsNullOrWhiteSpace(category) && category != "Popular")
            query = query.Where(d => d.Category == category);

        if (!string.IsNullOrWhiteSpace(searchText))
            query = query.Where(d => d.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                                   || d.VendorName.Contains(searchText, StringComparison.OrdinalIgnoreCase));

        return query;
    }

    public List<Vendor> TopSellers => Vendors.OrderByDescending(v => v.Rating).Take(3).ToList();

    public Vendor RegisterVendor(string businessName, string contact)
    {
        var vendor = new Vendor { Name = businessName, Rating = 0, TotalOrders = 0, PendingOrders = 0, Balance = 0 };
        Vendors.Add(vendor);
        return vendor;
    }
}
