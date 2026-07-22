namespace AfroPhloem.Models;

/// <summary>
/// enumeration representing the different delivery speeds available for shipment,
/// </summary>
public enum DeliverySpeed { Standard, Express, Bulk }

/// <summary>
/// enumeration representing the various shipment statuses that a delivery order can have,
/// </summary>
public enum ShipmentStatus { Processing, InTransit, Delivered }

/// <summary>
/// class representing a delivery order, encapsulating details such as 
/// sender and recipient information, delivery speed, item description, status, and associated fees.
/// </summary>
public class DeliveryOrder
{
    /// <summary>
    /// identifier for the delivery order, initialized with a new GUID to ensure uniqueness.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// sender's name for the delivery order, initialized as an empty string to ensure it is not null.
    /// </summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// recipient's name for the delivery order, initialized as an empty string to ensure it is not null.
    /// </summary>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    /// fixed sender's country for the delivery order, set to "Ghana" as the default value.
    /// </summary>
    public string FromCountry { get; set; } = "Ghana";

    /// <summary>
    /// address in the UK for the delivery order, initialized as an empty string to ensure it is not null.
    /// </summary>
    public string UkAddress { get; set; } = string.Empty;

    /// <summary>
    /// speed of delivery for the order, defaulting to standard delivery, which affects the associated fee.
    /// </summary>
    public DeliverySpeed Speed { get; set; } = DeliverySpeed.Standard;

    /// <summary>
    /// item description for the delivery order, initialized as an empty string to ensure it is not null.
    /// </summary>
    public string ItemDescription { get; set; } = string.Empty;

    /// <summary>
    /// status of the shipment, defaulting to processing, which indicates the current state of the delivery order.
    /// </summary>
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Processing;

    /// <summary>
    /// creation timestamp for the delivery order, initialized to the current date and time when the order is created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets the delivery fee based on the selected delivery speed.
    /// </summary>
    public decimal Fee => Speed switch
    {
        DeliverySpeed.Standard => 25.00m,
        DeliverySpeed.Express => 45.00m,
        DeliverySpeed.Bulk => 80.00m,
        _ => 25.00m
    };
}