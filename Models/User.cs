namespace AfroPhloem.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Country { get; set; } = "Ghana";
    public DateTime JoinedAt { get; set; } = DateTime.Now;
    public bool IsAdmin { get; set; }

    /// <summary>A user can save multiple delivery addresses (home, work, etc.) - one is marked default.</summary>
    public List<DeliveryAddress> Addresses { get; set; } = new();

    public DeliveryAddress? DefaultAddress =>
        Addresses.FirstOrDefault(a => a.IsDefault) ?? Addresses.FirstOrDefault();

    // DEMO ONLY: plaintext password stored in memory so mock Login can validate
    // against it. A real app must never store or transmit passwords like this -
    // they belong hashed (e.g. bcrypt/Argon2) behind a real auth backend.
    public string Password { get; set; } = string.Empty;
}