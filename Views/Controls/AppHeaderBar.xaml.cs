using System.Collections.ObjectModel;
using System.Windows.Input;
using AfroPhloem.Models;

namespace AfroPhloem.Views.Controls;

public partial class AppHeaderBar : ContentView
{
    public AppHeaderBar()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ShowBackButtonProperty =
    BindableProperty.Create(nameof(ShowBackButton), typeof(bool), typeof(AppHeaderBar), true);

    public bool ShowBackButton
    {
        get => (bool)GetValue(ShowBackButtonProperty);
        set => SetValue(ShowBackButtonProperty, value);
    }
    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Shell.Current is null) return;

        if (Shell.Current.Navigation.NavigationStack.Count > 1)
            await Shell.Current.Navigation.PopAsync();
        else
            await Shell.Current.GoToAsync("//home");
    }
    private async void OnCountryTapped(object? sender, TappedEventArgs e)
    {
        if (Shell.Current is null || Countries.Count == 0) return;

        var options = Countries.Select(c => c.Display).ToArray();
        var choice = await Shell.Current.DisplayActionSheet("Select Country", "Cancel", null, options);

        if (string.IsNullOrEmpty(choice) || choice == "Cancel") return;

        var match = Countries.FirstOrDefault(c => c.Display == choice);
        if (match is not null)
            SelectedCountry = match;
    }

    public static readonly BindableProperty CountriesProperty =
        BindableProperty.Create(nameof(Countries), typeof(ObservableCollection<CountryOption>), typeof(AppHeaderBar),
            new ObservableCollection<CountryOption>(CountryOption.All));

    public ObservableCollection<CountryOption> Countries
    {
        get => (ObservableCollection<CountryOption>)GetValue(CountriesProperty);
        set => SetValue(CountriesProperty, value);
    }

    public static readonly BindableProperty SelectedCountryProperty =
        BindableProperty.Create(nameof(SelectedCountry), typeof(CountryOption), typeof(AppHeaderBar),
            default(CountryOption), BindingMode.TwoWay);

    public CountryOption SelectedCountry
    {
        get => (CountryOption)GetValue(SelectedCountryProperty);
        set => SetValue(SelectedCountryProperty, value);
    }

    public static readonly BindableProperty ShowSearchProperty =
        BindableProperty.Create(nameof(ShowSearch), typeof(bool), typeof(AppHeaderBar), false);

    public bool ShowSearch
    {
        get => (bool)GetValue(ShowSearchProperty);
        set => SetValue(ShowSearchProperty, value);
    }

    public static readonly BindableProperty SearchTextProperty =
        BindableProperty.Create(nameof(SearchText), typeof(string), typeof(AppHeaderBar),
            string.Empty, BindingMode.TwoWay);

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    private void OnMenuClicked(object? sender, EventArgs e)
    {
        if (Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }
}
