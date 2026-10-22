using CommunityToolkit.Mvvm.ComponentModel;
using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Models;

/// <summary>
/// One choice in Checkout → "Delivery time". <see cref="IsOpen"/> is false when a shop in the basket doesn't take it
/// right now (e.g. after the shop's cut-off only Tomorrow is open).
/// </summary>
public partial class DeliveryTimeOption : ObservableObject
{
    public required DeliveryWindow Window { get; init; }

    public string Title => TextOf(Window);

    [ObservableProperty]
    public partial bool IsOpen { get; set; } = true;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    /// <summary>Every delivery time, in order.</summary>
    public static IReadOnlyList<DeliveryTimeOption> All() =>
        Enum.GetValues<DeliveryWindow>().Select(w => new DeliveryTimeOption { Window = w }).ToList();

    /// <summary>What a delivery time is called on screen.</summary>
    public static string TextOf(DeliveryWindow window) => window switch
    {
        DeliveryWindow.Asap => "As soon as possible",
        DeliveryWindow.TodayEvening => "This evening",
        DeliveryWindow.Tomorrow => "Tomorrow",
        _ => window.ToString()
    };
}
