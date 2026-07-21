using AfroPhloem.ViewModels;

namespace AfroPhloem.Views;

public partial class SellerSetupPage : ContentPage
{
    public SellerSetupPage(SellerSetupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
