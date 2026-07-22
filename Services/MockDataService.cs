using System.Collections.ObjectModel;
using AfroPhloem.Models;

namespace AfroPhloem.Services;

/// <summary>
/// In-memory data source standing in for a real API. Swap the methods below
/// for HttpClient calls to your backend when one is ready — the ViewModels
/// only depend on this class's public surface, so nothing else needs to change.
/// </summary>
public class MockDataService
{
    public List<User> Users { get; } = new()
    {
        // DEMO ONLY: plaintext passwords, see the note on User.Password.
        new User { FullName = "Efua Owusu",   Email = "efua.owusu@gmail.com",    PhoneNumber = "+233 24 555 0192", Country = "Ghana",          Password = "password123", JoinedAt = DateTime.Now.AddMonths(-11),
            Addresses = new() { new DeliveryAddress { Label = "Home", FullAddress = "12 Ring Road, Accra", Country = "Ghana", IsDefault = true } } },
        new User { FullName = "Tunde Bakare", Email = "tunde.bakare@yahoo.com",  PhoneNumber = "+234 803 555 0147", Country = "Nigeria",        Password = "password123", JoinedAt = DateTime.Now.AddMonths(-5),
            Addresses = new() { new DeliveryAddress { Label = "Home", FullAddress = "5 Allen Avenue, Lagos", Country = "Nigeria", IsDefault = true } } },
        new User { FullName = "Grace Adjei",  Email = "grace.adjei@outlook.com", PhoneNumber = "+44 7700 900123",  Country = "United Kingdom", Password = "password123", JoinedAt = DateTime.Now.AddDays(-20),
            Addresses = new()
            {
                new DeliveryAddress { Label = "Home", FullAddress = "31 Camden High St, London", Country = "United Kingdom", IsDefault = true },
                new DeliveryAddress { Label = "Work", FullAddress = "9 Canary Wharf, London", Country = "United Kingdom" }
            } },
        new User { FullName = "Admin",        Email = "admin@phloem.com",        PhoneNumber = "",                 Country = "Ghana",          Password = "admin123", IsAdmin = true, JoinedAt = DateTime.Now.AddYears(-1) },
    };

    public List<Vendor> Vendors { get; } = new()
    {
        // Legacy seed vendors predate user-linked accounts, so UserId is left empty.
        new Vendor { Name = "Mama Ama's Kitchen", Rating = 4.8, Country = "Ghana",  TotalOrders = 120, PendingOrders = 5, Balance = 3400m, IsVerified = true,  IsApproved = true, BusinessEmail = "ama@mamaama.com", JoinedAt = DateTime.Now.AddMonths(-14) },
        new Vendor { Name = "Lagos Flavours",      Rating = 4.6, Country = "Nigeria", TotalOrders = 86,  PendingOrders = 2, Balance = 1980m, IsVerified = true,  IsApproved = true, BusinessEmail = "hello@lagosflavours.com", JoinedAt = DateTime.Now.AddMonths(-9) },
        new Vendor { Name = "Diaspora Delights",   Rating = 4.9, Country = "United Kingdom", TotalOrders = 42, PendingOrders = 1, Balance = 950m, IsVerified = false, IsApproved = true, BusinessEmail = "info@diasporadelights.co.uk", JoinedAt = DateTime.Now.AddMonths(-4) },
        new Vendor { Name = "Food Affairs Ltd",    Rating = 4.7, Country = "Ghana",  TotalOrders = 64,  PendingOrders = 3, Balance = 2150m, IsVerified = true,  IsApproved = true, BusinessEmail = "contact@foodaffairs.gh", JoinedAt = DateTime.Now.AddMonths(-6) },

        // Awaiting admin approval - not yet visible to buyers
        new Vendor { Name = "Naija Bites",         Rating = 0,   Country = "Nigeria", TotalOrders = 0, PendingOrders = 0, Balance = 0m, IsVerified = false, IsApproved = false, BusinessEmail = "sign-up@naijabites.com", JoinedAt = DateTime.Now.AddDays(-2) },
        new Vendor { Name = "Accra Street Eats",   Rating = 0,   Country = "Ghana",   TotalOrders = 0, PendingOrders = 0, Balance = 0m, IsVerified = false, IsApproved = false, BusinessEmail = "owner@accrastreeteats.com", JoinedAt = DateTime.Now.AddDays(-1) },
    };

    public List<Dish> Dishes { get; }

    public ObservableCollection<DeliveryOrder> Shipments { get; } = new()
    {
        new DeliveryOrder { SenderName = "Kwame Boateng", RecipientName = "Adjoa Boateng", FromCountry = "Ghana", UkAddress = "14 Elm Road, Manchester", ItemDescription = "Dried fish, gari, spices", Speed = DeliverySpeed.Express, Status = ShipmentStatus.Delivered, CreatedAt = DateTime.Now.AddDays(-9) },
        new DeliveryOrder { SenderName = "Chidinma Okafor", RecipientName = "Ifeoma Eze", FromCountry = "Nigeria", UkAddress = "8 Baker Street, London", ItemDescription = "Egusi, palm oil, suya spice", Speed = DeliverySpeed.Standard, Status = ShipmentStatus.InTransit, CreatedAt = DateTime.Now.AddDays(-3) },
        new DeliveryOrder { SenderName = "Kojo Mensah", RecipientName = "Abena Mensah", FromCountry = "Ghana", UkAddress = "22 Queens Ave, Birmingham", ItemDescription = "Waakye beans, shito", Speed = DeliverySpeed.Bulk, Status = ShipmentStatus.Processing, CreatedAt = DateTime.Now.AddHours(-14) },
    };

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
        var approvedVendorIds = Vendors.Where(v => v.IsApproved && !v.IsBlocked).Select(v => v.Id).ToHashSet();
        var query = Dishes.Where(d => approvedVendorIds.Contains(d.VendorId));

        if (!string.IsNullOrWhiteSpace(country) && country != "All")
            query = query.Where(d => d.Country == country);

        if (!string.IsNullOrWhiteSpace(category) && category != "Popular")
            query = query.Where(d => d.Category == category);

        if (!string.IsNullOrWhiteSpace(searchText))
            query = query.Where(d => d.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                                   || d.VendorName.Contains(searchText, StringComparison.OrdinalIgnoreCase));

        return query;
    }

    /// <summary>Only approved, non-blocked vendors should ever be visible to buyers.</summary>
    public List<Vendor> TopSellers => Vendors
        .Where(v => v.IsApproved && !v.IsBlocked)
        .OrderByDescending(v => v.Rating)
        .Take(3)
        .ToList();

    /// <summary>
    /// Creates a vendor profile linked to an existing logged-in user. Business email/phone
    /// fall back to the user's personal ones when left blank ("if different from your personal one").
    /// </summary>
    public Vendor RegisterVendor(string businessName, User owner, string? businessEmail, string? businessPhone)
    {
        var vendor = new Vendor
        {
            Name = businessName,
            UserId = owner.Id,
            BusinessEmail = string.IsNullOrWhiteSpace(businessEmail) ? owner.Email : businessEmail,
            BusinessPhone = string.IsNullOrWhiteSpace(businessPhone) ? owner.PhoneNumber : businessPhone,
            Country = owner.Country,
            Rating = 0,
            TotalOrders = 0,
            PendingOrders = 0,
            Balance = 0,
            IsApproved = false, // awaiting admin review
            JoinedAt = DateTime.Now
        };
        Vendors.Add(vendor);
        return vendor;
    }

    public User RegisterUser(string fullName, string email, string phone, string address, string country, string password)
    {
        var user = new User
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phone,
            Country = country,
            Password = password,
            JoinedAt = DateTime.Now
        };

        if (!string.IsNullOrWhiteSpace(address))
        {
            user.Addresses.Add(new DeliveryAddress
            {
                Label = "Home",
                FullAddress = address,
                Country = country,
                IsDefault = true
            });
        }

        Users.Add(user);
        return user;
    }

    public User? Authenticate(string email, string password) =>
        Users.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase) && u.Password == password);

    public void AddShipment(DeliveryOrder order) => Shipments.Insert(0, order);
}