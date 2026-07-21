namespace AfroPhloem.Models;

public class Vendor
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = "dotnet_bot.png";
    public double Rating { get; set; }
    public string Country { get; set; } = "Ghana";
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public decimal Balance { get; set; }
    public bool IsVerified { get; set; }
    public int DishCount { get; set; }
}
