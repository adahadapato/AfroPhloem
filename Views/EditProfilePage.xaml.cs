using Phloem.ViewModels;

namespace Phloem.Views;

public partial class EditProfilePage : ContentPage
{
    readonly EditProfileViewModel _vm;

    public EditProfilePage(EditProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Load();   // fresh copy of the signed-in user's details
    }

    async void OnBackTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");
}
