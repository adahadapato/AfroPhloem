using CommunityToolkit.Mvvm.ComponentModel;

namespace AfroPhloem.Models;

public partial class CartItem : ObservableObject
{
    public Dish Dish { get; set; } = null!;

    [ObservableProperty]
    private int quantity = 1;

    public decimal LineTotal => Dish.Price * Quantity;

    partial void OnQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(LineTotal));
    }
}
