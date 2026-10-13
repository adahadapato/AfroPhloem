using Phloem.ViewModels;

namespace Phloem.Views;

public partial class VendorsListPage : ContentPage
{
    readonly VendorsListViewModel _vm;

    public VendorsListPage(VendorsListViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Pick up newly approved shops and the latest country choice
        _vm.Reload();
    }

    // ☰ opens the side menu
    void OnMenuTapped(object? sender, TappedEventArgs e)
    {
        if (Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = true;
    }

    // Flag pill: change the app's country (Ghana / Nigeria / UK)
    async void OnCountryTapped(object? sender, TappedEventArgs e)
    {
        var names = _vm.Countries.Select(c => $"{c.FlagEmoji}  {c.Name}").ToArray();
        var choice = await DisplayActionSheet("Choose your country", "Cancel", null, names);

        var picked = _vm.Countries.FirstOrDefault(c => $"{c.FlagEmoji}  {c.Name}" == choice);
        if (picked is not null)
            _vm.SelectedCountry = picked;
    }
}
