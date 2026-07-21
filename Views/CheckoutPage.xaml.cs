using AfroPhloem.ViewModels;

namespace AfroPhloem.Views;

public partial class CheckoutPage : ContentPage
{
    public CheckoutPage(CheckoutViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
