using System.Collections.ObjectModel;
using System.Globalization;
using AfroPhloem.Models;

namespace AfroPhloem.Services;

public class CartService
{
    public ObservableCollection<CartItem> Items { get; } = new();

    public CountryOption SelectedCountry { get; set; }

    public DeliveryOrder? PendingInternationalDelivery { get; set; }

    public CartService()
    {
        SelectedCountry = DetectDefaultCountry();
    }

    /// <summary>
    /// Uses the device's regional setting to pick a sensible default market.
    /// Falls back to Ghana if the device's region isn't one we operate in
    /// (e.g. someone testing from the US or a region with no locale set).
    /// </summary>
    private static CountryOption DetectDefaultCountry()
    {
        try
        {
            var deviceRegionCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
            var match = CountryOption.All.FirstOrDefault(c =>
                string.Equals(c.IsoCode, deviceRegionCode, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
                return match;
        }
        catch
        {
            // RegionInfo can occasionally throw on some platform configurations; fall through to default.
        }

        return CountryOption.All[0]; // Ghana
    }

    public void AddDish(Dish dish)
    {
        var existing = Items.FirstOrDefault(i => i.Dish.Id == dish.Id);
        if (existing is not null)
        {
            existing.Quantity++;
        }
        else
        {
            Items.Add(new CartItem { Dish = dish, Quantity = 1 });
        }
    }

    public void RemoveItem(CartItem item) => Items.Remove(item);

    public decimal Subtotal => Items.Sum(i => i.LineTotal);

    public decimal DeliveryFee => PendingInternationalDelivery?.Fee ?? (Items.Count > 0 ? 3.50m : 0m);

    public decimal Total => Subtotal + DeliveryFee;

    public void Clear()
    {
        Items.Clear();
        PendingInternationalDelivery = null;
    }
}
