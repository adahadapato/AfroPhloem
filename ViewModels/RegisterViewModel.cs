using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfroPhloem.Services;

namespace AfroPhloem.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isSellerRole; // false = Buyer, true = Seller

    [ObservableProperty]
    private string fullName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool agreedToTerms;

    [ObservableProperty]
    private string shopLogoFileName = string.Empty; // only relevant when IsSellerRole

    [ObservableProperty]
    private string statusMessage = string.Empty;

    private readonly MockDataService _data;

    public RegisterViewModel(MockDataService data)
    {
        _data = data;
    }

    [RelayCommand]
    private void SelectBuyer() => IsSellerRole = false;

    [RelayCommand]
    private void SelectSeller() => IsSellerRole = true;

    [RelayCommand]
    private async Task PickShopLogo()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result is not null)
                ShopLogoFileName = result.FileName;
        }
        catch
        {
            // platform without photo picker support (e.g. some desktop configs) — ignore quietly
        }
    }

    [RelayCommand]
    private async Task SignUp()
    {
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Please fill in all required fields.";
            return;
        }

        if (!AgreedToTerms)
        {
            StatusMessage = "Please accept the Terms & Conditions.";
            return;
        }

        if (IsSellerRole)
        {
            _data.RegisterVendor(FullName, Email);
            StatusMessage = $"Vendor account created for {FullName}.";
            await Shell.Current.GoToAsync("//sellersetup");
        }
        else
        {
            StatusMessage = $"Welcome, {FullName}! Your buyer account is ready.";
            await Shell.Current.GoToAsync("//home");
        }
    }
}
