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
