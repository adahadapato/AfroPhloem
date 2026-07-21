using AfroPhloem.ViewModels;

namespace AfroPhloem.Views;

public partial class VendorsListPage : ContentPage
{
    public VendorsListPage(VendorsListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}