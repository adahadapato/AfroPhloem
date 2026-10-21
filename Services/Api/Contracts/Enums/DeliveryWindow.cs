namespace Phloem.Services.Api.Contracts.Enums;

/// <summary>When the buyer wants the food. Same names as the API's <c>DeliveryWindow</c>.</summary>
public enum DeliveryWindow
{
    /// <summary>As soon as possible.</summary>
    Asap = 1,

    /// <summary>This evening.</summary>
    TodayEvening = 2,

    /// <summary>Tomorrow.</summary>
    Tomorrow = 3
}
