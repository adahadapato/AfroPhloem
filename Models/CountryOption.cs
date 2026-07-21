namespace AfroPhloem.Models;

public class CountryOption
{
    public string Name { get; set; } = string.Empty;
    public string FlagEmoji { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;
    public bool SupportsInternationalDelivery { get; set; }

    public string Display => $"{FlagEmoji}  {Name}";

    public static List<CountryOption> All => new()
    {
        new CountryOption { Name = "Ghana",          FlagEmoji = "🇬🇭", CurrencySymbol = "₵",  IsoCode = "GH", SupportsInternationalDelivery = true },
        new CountryOption { Name = "Nigeria",        FlagEmoji = "🇳🇬", CurrencySymbol = "₦",  IsoCode = "NG", SupportsInternationalDelivery = true },
        new CountryOption { Name = "United Kingdom", FlagEmoji = "🇬🇧", CurrencySymbol = "£",  IsoCode = "GB", SupportsInternationalDelivery = false },
    };
}
