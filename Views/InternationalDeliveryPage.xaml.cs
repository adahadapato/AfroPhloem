using AfroPhloem.ViewModels;

namespace AfroPhloem.Views;

public partial class InternationalDeliveryPage : ContentPage
{
    public InternationalDeliveryPage(InternationalDeliveryViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
