using AfroPhloem.ViewModels;

namespace AfroPhloem.Views;

public partial class FoodListingPage : ContentPage, IQueryAttributable
{
    private readonly FoodListingViewModel _vm;

    public FoodListingPage(FoodListingViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("vendorId", out var vendorId))
        {
            query.TryGetValue("vendorName", out var vendorName);
            _vm.SetVendorFilter(vendorId?.ToString(), vendorName?.ToString());
        }
        else
        {
            // Navigated here without a vendor filter (e.g. from the flyout menu) - clear any previous filter.
            _vm.SetVendorFilter(null, null);
        }
    }
}