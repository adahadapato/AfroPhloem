namespace Phloem.Services.Api.Contracts;

/// <summary>How a dish search is sorted. Same names and numbers as the API.</summary>
public enum DishSort
{
    /// <summary>Best rated first (the default).</summary>
    Rating = 1,

    /// <summary>Cheapest first.</summary>
    PriceLow = 2,

    /// <summary>Most expensive first.</summary>
    PriceHigh = 3
}
