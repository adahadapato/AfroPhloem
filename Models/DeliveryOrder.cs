namespace AfroPhloem.Models;

public enum DeliverySpeed { Standard, Express, Bulk }

public class DeliveryOrder
{
    public string SenderName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string UkAddress { get; set; } = string.Empty;
    public DeliverySpeed Speed { get; set; } = DeliverySpeed.Standard;
    public string ItemDescription { get; set; } = string.Empty;

    public decimal Fee => Speed switch
    {
        DeliverySpeed.Standard => 25.00m,
        DeliverySpeed.Express => 45.00m,
        DeliverySpeed.Bulk => 80.00m,
        _ => 25.00m
    };
}
