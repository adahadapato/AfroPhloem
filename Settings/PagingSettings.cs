namespace Phloem.Settings;

/// <summary>
/// The lists shown a page at a time. Each name is a key under "Paging:Defaults" in appsettings.json and the key the
/// person's own "Show" choice is remembered under on the phone.
/// </summary>
public static class PagedLists
{
    public const string Dishes = nameof(Dishes);
    public const string Shops = nameof(Shops);
    public const string MyOrders = nameof(MyOrders);
    public const string MyShipments = nameof(MyShipments);
    public const string SellerOrders = nameof(SellerOrders);
    public const string SellerDishes = nameof(SellerDishes);
    public const string AdminShops = nameof(AdminShops);
    public const string AdminUsers = nameof(AdminUsers);
    public const string AdminShipments = nameof(AdminShipments);
    public const string AdminDishNames = nameof(AdminDishNames);
}

/// <summary>
/// The "Paging" section of appsettings.json: the "Show" choices and how many items each list starts with.
/// Change the numbers there; no code changes needed.
/// </summary>
public sealed class PagingSettings
{
    public const string SectionName = AppSettings.Paging.Section;

    /// <summary>The API's limit per page.</summary>
    public const int MaxPageSize = 100;

    /// <summary>Used when the settings file has no (valid) choices.</summary>
    public static readonly IReadOnlyList<int> FallbackSizeOptions = [10, 15, 20, 30, 50];

    /// <summary>Used for a list with no (valid) default.</summary>
    public const int FallbackDefaultSize = 20;

    /// <summary>The "Show" choices, e.g. [10, 15, 20, 30, 50].</summary>
    public int[] SizeOptions { get; set; } = [];

    /// <summary>Items per page per list (keys are <see cref="PagedLists"/>), until the person picks their own.</summary>
    public Dictionary<string, int> Defaults { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The "Show" choices for a list: the configured ones (1–100, smallest first) plus the list's default.</summary>
    public IReadOnlyList<int> OptionsFor(string list)
    {
        var options = SizeOptions.Where(IsValid).ToList();
        if (options.Count == 0) options.AddRange(FallbackSizeOptions);
        options.Add(DefaultSizeFor(list));
        return options.Distinct().Order().ToList();
    }

    /// <summary>How many items a list starts with.</summary>
    public int DefaultSizeFor(string list) =>
        Defaults.TryGetValue(list, out var size) && IsValid(size) ? size : FallbackDefaultSize;

    private static bool IsValid(int size) => size is >= 1 and <= MaxPageSize;
}
