using Phloem.ViewModels;

namespace Phloem.Views;

public partial class NotificationSettingsPage : ContentPage
{
    readonly NotificationSettingsViewModel _vm;

    public NotificationSettingsPage(NotificationSettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Load();
    }

    async void OnBackTapped(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");
}
