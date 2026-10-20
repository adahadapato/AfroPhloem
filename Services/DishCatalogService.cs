using System.Collections.ObjectModel;

namespace Phloem.Services;

/// <summary>A dish name sellers can pick from (or one suggested by a seller, waiting for admin).</summary>
public record CatalogDish(string Name, string Category, string Group)
{
    /// <summary>Flag / icon shown next to the group in the picker.</summary>
    public string GroupIcon => Group switch
    {
        "Ghana" => "🇬🇭",
        "Nigeria" => "🇳🇬",
        "Drinks" => "🥤",
        "Snacks" => "🍪",
        _ => "🌍"
    };

    // Set only for seller suggestions
    public string? SuggestedBy { get; init; }
    public DateTime SuggestedAt { get; init; } = DateTime.Now;
}

/// <summary>
/// The fixed list of dish names (so there are no typos), plus new names suggested by sellers
/// that an admin approves. In-memory for now; your backend can replace this later.
/// Register as a singleton in MauiProgram.cs.
/// </summary>
public class DishCatalogService
{
    // Category values match the app's chips: "Local Meals", "Snacks", "Drinks"
    public List<CatalogDish> Dishes { get; } = new()
    {
        // 🇬🇭 Ghana
        new("Jollof Rice",          "Local Meals", "Ghana"),
        new("Waakye",               "Local Meals", "Ghana"),
        new("Banku & Tilapia",      "Local Meals", "Ghana"),
        new("Kenkey & Fish",        "Local Meals", "Ghana"),
        new("Fufu & Light Soup",    "Local Meals", "Ghana"),
        new("Red Red",              "Local Meals", "Ghana"),
        new("Ampesi",               "Local Meals", "Ghana"),
        new("Omo Tuo",              "Local Meals", "Ghana"),
        new("Kelewele",             "Snacks",      "Ghana"),

        // 🇳🇬 Nigeria
        new("Jollof Rice",          "Local Meals", "Nigeria"),
        new("Egusi Soup",           "Local Meals", "Nigeria"),
        new("Pounded Yam & Egusi",  "Local Meals", "Nigeria"),
        new("Ofada Rice",           "Local Meals", "Nigeria"),
        new("Pepper Soup",          "Local Meals", "Nigeria"),
        new("Moi Moi",              "Local Meals", "Nigeria"),
        new("Suya",                 "Snacks",      "Nigeria"),
        new("Akara",                "Snacks",      "Nigeria"),
        new("Puff Puff",            "Snacks",      "Nigeria"),
        new("Chin Chin",            "Snacks",      "Nigeria"),

        // 🌍 West African
        new("Fried Rice",           "Local Meals", "West African"),
        new("Fried Plantain",       "Local Meals", "West African"),
        new("Groundnut Soup",       "Local Meals", "West African"),

        // 🍪 Snacks
        new("Meat Pie",             "Snacks",      "Snacks"),
        new("Plantain Chips",       "Snacks",      "Snacks"),
        new("Coconut Candy",        "Snacks",      "Snacks"),

        // 🥤 Drinks
        new("Sobolo",               "Drinks",      "Drinks"),
        new("Zobo",                 "Drinks",      "Drinks"),
        new("Palm Wine",            "Drinks",      "Drinks"),
        new("Kunu",                 "Drinks",      "Drinks"),
        new("Asaana",               "Drinks",      "Drinks"),
    };

    /// <summary>New names sellers asked for, waiting for an admin (shown on the Admin dashboard).</summary>
    public ObservableCollection<CatalogDish> Suggestions { get; } = new();

    public void Suggest(string name, string category, string group, string suggestedBy)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;
        if (Dishes.Any(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) return;
        if (Suggestions.Any(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) return;

        Suggestions.Add(new CatalogDish(name, category, group) { SuggestedBy = suggestedBy });
    }

    public void Approve(CatalogDish suggestion)
    {
        Suggestions.Remove(suggestion);
        Dishes.Add(suggestion with { SuggestedBy = null });
    }

    public void Reject(CatalogDish suggestion) => Suggestions.Remove(suggestion);

    /// <summary>
    /// Finds the list name a dish was built from, e.g. "Jollof Rice &amp; Chicken" → "Jollof Rice" + "&amp; Chicken".
    /// </summary>
    public (CatalogDish? Base, string Detail) Split(string fullName, string? country)
    {
        var match = Dishes
            .Where(d => fullName.StartsWith(d.Name, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(d => d.Name.Length)
            .ThenByDescending(d => d.Group == country)
            .FirstOrDefault();

        return match is null ? (null, string.Empty) : (match, fullName[match.Name.Length..].Trim());
    }
}
