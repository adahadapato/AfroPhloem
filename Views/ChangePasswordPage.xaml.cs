using Phloem.ViewModels;

namespace Phloem.Views;

public partial class ChangePasswordPage : ContentPage
{
    readonly ChangePasswordViewModel _vm;

    public ChangePasswordPage(ChangePasswordViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Reset();   // never keep old passwords in the boxes
    }

    async void OnBackTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");
}
