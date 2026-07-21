using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfroPhloem.Services;

namespace AfroPhloem.ViewModels;

public partial class SellerSetupViewModel : ObservableObject
{
    private readonly MockDataService _data;

    [ObservableProperty]
    private string businessName = string.Empty;

    [ObservableProperty]
    private string contactDetails = string.Empty;

    [ObservableProperty]
    private string logoFileName = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public SellerSetupViewModel(MockDataService data)
    {
        _data = data;
    }

    [RelayCommand]
    private async Task PickLogo()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result is not null)
                LogoFileName = result.FileName;
        }
        catch
        {
            // ignore on platforms without a media picker
        }
    }

    [RelayCommand]
    private async Task CreateVendorProfile()
    {
        if (string.IsNullOrWhiteSpace(BusinessName))
        {
            StatusMessage = "Business name is required.";
            return;
        }

        _data.RegisterVendor(BusinessName, ContactDetails);
        StatusMessage = $"{BusinessName} is now listed on Afro-Phloem.";
        await Shell.Current.GoToAsync("//dashboard");
    }
}
