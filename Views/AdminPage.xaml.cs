using AfroPhloem.ViewModels;

namespace AfroPhloem.Views;

public partial class AdminPage : ContentPage
{
    public AdminPage(AdminViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}