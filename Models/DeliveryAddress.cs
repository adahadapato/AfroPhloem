namespace AfroPhloem.Models;

public class DeliveryAddress
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Label { get; set; } = "Home"; // e.g. Home, Work, Family House
    public string FullAddress { get; set; } = string.Empty;
    public string Country { get; set; } = "Ghana";
    public bool IsDefault { get; set; }
}