using Phloem.Models;

namespace Phloem.Services;

/// <summary>
/// Remembers which address the current order should go to.
/// Register as a singleton in MauiProgram.cs.
/// </summary>
public class DeliveryAddressService
{
    /// <summary>The address picked on the Delivery address page (null = use the default).</summary>
    public DeliveryAddress? SelectedAddress { get; set; }

    /// <summary>
    /// The address to deliver to: the one picked on the Delivery address page if it belongs to
    /// this user (or the user is a guest), otherwise the user's default, otherwise their first address.
    /// </summary>
    public DeliveryAddress? Resolve(User? user)
    {
        if (SelectedAddress is not null && (user is null || user.Addresses.Contains(SelectedAddress)))
            return SelectedAddress;

        return user?.Addresses.FirstOrDefault(a => a.IsDefault) ?? user?.Addresses.FirstOrDefault();
    }

    /// <summary>"Home · 31 Camden High St, London", or a prompt when there is no address yet.</summary>
    public string Describe(User? user)
    {
        var a = Resolve(user);
        return a is null ? "No delivery address yet" : $"{a.Label} · {a.FullAddress}";
    }
}
