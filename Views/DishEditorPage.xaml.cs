using Phloem.ViewModels;

namespace Phloem.Views;

public partial class DishEditorPage : ContentPage
{
    readonly DishEditorViewModel _vm;

    public DishEditorPage(DishEditorViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // My shop and the dish list with today's allowed prices
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await _vm.LoadAsync(); }
        catch (Exception) { /* the page says what went wrong */ }
    }

    // ← back (closes the dish list first if it's open)
    async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        if (_vm.IsPickerOpen) { _vm.IsPickerOpen = false; return; }
        await Shell.Current.GoToAsync("..");
    }

    // Android back button: close the dish list first
    protected override bool OnBackButtonPressed()
    {
        if (_vm.IsPickerOpen) { _vm.IsPickerOpen = false; return true; }
        return base.OnBackButtonPressed();
    }
}
