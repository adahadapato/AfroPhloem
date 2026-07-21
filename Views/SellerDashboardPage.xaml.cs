using AfroPhloem.ViewModels;

namespace AfroPhloem.Views;

public partial class SellerDashboardPage : ContentPage
{
    public SellerDashboardPage(SellerDashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
