using CommunityToolkit.Mvvm.ComponentModel;


namespace AfroPhloem.Models;

public partial class CartItem : ObservableObject
{
    public Dish Dish { get; set; } = null!;

    [ObservableProperty]
    public partial int Quantity { get; set; } = 1;

    public decimal LineTotal => Dish.Price * Quantity;

    partial void OnQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(LineTotal));
    }
}