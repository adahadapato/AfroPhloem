using Phloem.ViewModels;

namespace Phloem.Views;

public partial class AdminDishCatalogPage : ContentPage
{
    readonly AdminDishCatalogViewModel _vm;

    public AdminDishCatalogPage(AdminDishCatalogViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;

        // Tapping a name lower down opens the form at the top: bring it into view
        _vm.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName != nameof(AdminDishCatalogViewModel.IsEditing) || !_vm.IsEditing) return;
            try { await PageScroll.ScrollToAsync(0, 0, true); }
            catch (Exception) { /* scrolling isn't essential */ }
        };
    }

    // The names and ranges as they are now (new names may have been approved)
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _vm.LoadAsync(); }
        catch (Exception) { /* the page says what went wrong; pull down to try again */ }
    }

    async void OnBackTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");
}
