using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Phloem.Models;
using Phloem.Services;
using Phloem.Services.Api;
using Phloem.Services.Api.Contracts.Commands;
using Phloem.Services.Api.Contracts.Dtos;
using Phloem.Services.Api.Contracts.Enums;
using Phloem.Settings;

namespace Phloem.ViewModels;

/// <summary>One document row in Shop settings → Documents: what it is, where it stands, Upload / Replace.</summary>
public sealed class ShopDocumentRow
{
    public required VendorDocumentType Type { get; init; }
    public required bool IsRequired { get; init; }
    public MyShopDocumentDto? Document { get; init; }

    public string Title => Type switch
    {
        VendorDocumentType.BusinessRegistration => "Business registration",
        VendorDocumentType.FoodHygieneCertificate => "Food hygiene certificate",
        VendorDocumentType.OwnerId => "Owner's photo ID",
        _ => "Other document"
    };

    public string RequiredText => IsRequired ? "Needed for the ✓ badge" : "Optional";

    public bool IsAccepted => Document is { Status: DocumentStatus.Accepted, IsExpired: false };
    public bool NeedsAttention => Document is null ? IsRequired
                                  : Document.IsExpired || Document.Status == DocumentStatus.Rejected;

    public string StatusText => Document switch
    {
        null => "Not uploaded",
        { IsExpired: true } => "Expired: please upload a new one",
        { Status: DocumentStatus.Pending } => "Waiting for Phloem to check it",
        { Status: DocumentStatus.Accepted } => "Accepted ✓",
        { Status: DocumentStatus.Rejected } rejected => $"Rejected: {rejected.RejectionReason}",
        { } other => other.Status.ToString()
    };

    /// <summary>"gh-registration.pdf · expires 31 Dec 2027".</summary>
    public string FileText => Document is null
        ? string.Empty
        : Document.ExpiresAt is { } expires
            ? $"{Document.OriginalFileName} · expires {expires.ToLocalTime():d MMM yyyy}"
            : Document.OriginalFileName;

    public bool HasFile => Document is not null;
    public string ButtonText => Document is null ? "Upload" : "Replace";
}

/// <summary>
/// Shop settings (route "shopsettings"), on the API:
/// <list type="bullet">
///   <item><description>Approval status (and why a shop was rejected).</description></item>
///   <item><description>Logo and cover photo.</description></item>
///   <item><description>Name, business email and phone.</description></item>
///   <item><description>Delivery fee, inside the country's range.</description></item>
///   <item><description>"Orders for today close at" (default 7pm, the shop's local time).</description></item>
///   <item><description>Documents for Phloem to check (needed for the ✓ badge).</description></item>
/// </list>
/// Personal details stay on My account → Edit profile.
/// </summary>
public partial class ShopSettingsViewModel : ObservableObject
{
    private static readonly FilePickerFileType DocumentTypes = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        [DevicePlatform.Android] = ["application/pdf", "image/jpeg", "image/png"],
        [DevicePlatform.iOS] = ["com.adobe.pdf", "public.jpeg", "public.png"],
        [DevicePlatform.MacCatalyst] = ["com.adobe.pdf", "public.jpeg", "public.png"],
        [DevicePlatform.WinUI] = [".pdf", ".jpg", ".jpeg", ".png"]
    });

    private readonly SellerService _seller;
    private readonly ApiSettings _api;
    private bool _busy;

    public ShopSettingsViewModel(SellerService seller, IOptions<ApiSettings> api)
    {
        _seller = seller;
        _api = api.Value;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasShop), nameof(ShopCountry), nameof(ShopSince), nameof(ApprovalText),
                              nameof(IsLive), nameof(IsNotLive), nameof(LogoUrl), nameof(CoverPhotoUrl),
                              nameof(HasLogo), nameof(HasCoverPhoto), nameof(CurrencySymbol))]
    public partial MyShopDto? Shop { get; set; }

    [ObservableProperty] public partial string BusinessName { get; set; } = string.Empty;
    [ObservableProperty] public partial string BusinessEmail { get; set; } = string.Empty;
    [ObservableProperty] public partial string BusinessPhone { get; set; } = string.Empty;

    [ObservableProperty] public partial string StatusMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsError { get; set; }
    [ObservableProperty] public partial bool IsLoading { get; set; }

    public bool HasShop => Shop is not null;
    public string ShopCountry => Shop is null ? string.Empty : CountryOption.NameOf(Shop.Country);
    public string ShopSince => Shop is null ? string.Empty : $"On Phloem since {Shop.OpenedAt.ToLocalTime():MMMM yyyy}";
    public string CurrencySymbol => Shop is null ? string.Empty : OrderService.SymbolOf(Shop.Currency);

    /// <summary>Approved / waiting / rejected / blocked line under the title.</summary>
    public string ApprovalText => Shop switch
    {
        null => string.Empty,
        { Status: VendorStatus.PendingApproval } => "Waiting for Phloem to approve your shop: buyers can't see it yet.",
        { Status: VendorStatus.Rejected } shop =>
            $"Not approved: {shop.RejectionReason} Fix it below and save, and Phloem will look again.",
        { Status: VendorStatus.Blocked } => "Blocked: buyers can't see your shop. Please contact Phloem support.",
        { Status: VendorStatus.Closed } => "Closed: buyers can't see your shop.",
        { IsLive: false } => "Hidden: your account is suspended, so buyers can't see your shop.",
        { IsVerified: true } => "Approved and verified ✓: buyers can see your shop and dishes.",
        _ => "Approved: buyers can see your shop and dishes. Upload your documents for the ✓ badge."
    };

    public bool IsLive => Shop?.IsLive ?? false;
    public bool IsNotLive => !IsLive;

    // ---------- Pictures ----------
    public string LogoUrl => _api.ResolveFileUrl(Shop?.LogoUrl) ?? Vendor.DefaultLogo;
    public string CoverPhotoUrl => _api.ResolveFileUrl(Shop?.CoverPhotoUrl) ?? Vendor.DefaultCoverPhoto;
    public bool HasLogo => Shop?.LogoUrl is not null;
    public bool HasCoverPhoto => Shop?.CoverPhotoUrl is not null;

    // ---------- Delivery fee ----------
    [ObservableProperty] public partial string DeliveryFeeText { get; set; } = string.Empty;

    /// <summary>"Between GH₵10 and GH₵35 in Ghana".</summary>
    [ObservableProperty] public partial string FeeRangeText { get; set; } = string.Empty;

    private MyDeliveryFeeDto? _fee;

    // ---------- Order cut-off ----------
    /// <summary>"Orders for today close at" (the TimePicker's value).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CutoffHint))]
    public partial TimeSpan OrderCutoff { get; set; } = new(19, 0, 0);

    public string CutoffHint =>
        $"After {DateTime.Today.Add(OrderCutoff):h:mm tt} ({ShopCountry} time), customers can only choose Tomorrow.";

    // ---------- Documents ----------
    public ObservableCollection<ShopDocumentRow> Documents { get; } = new();

    // =================================================================

    /// <summary>Called each time the page appears.</summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var shop = await _seller.GetMyShopAsync();
            if (shop.IsFailure)
            {
                Shop = null;
                ShowError(shop.Error!.Status == 404
                    ? "You don't have a shop yet. Open one from \"Become a Seller\"."
                    : shop.Error.DisplayMessage);
                return;
            }

            ShowShop(shop.Value!);

            var fee = await _seller.GetDeliveryFeeAsync();
            if (fee.IsSuccess)
            {
                _fee = fee.Value!;
                DeliveryFeeText = _fee.CurrentFee.ToString("0.##");
                var symbol = OrderService.SymbolOf(_fee.Currency);
                FeeRangeText = $"Between {symbol}{_fee.MinAllowed:0.##} and {symbol}{_fee.MaxAllowed:0.##} in {ShopCountry}." +
                               (_fee.UsesCountryMinimum ? " You haven't set one, so the lowest is used." : string.Empty);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowShop(MyShopDto shop)
    {
        Shop = shop;
        BusinessName = shop.Name;
        BusinessEmail = shop.BusinessEmail;
        BusinessPhone = shop.BusinessPhone ?? string.Empty;
        OrderCutoff = shop.OrderCutoffTime.ToTimeSpan();
        OnPropertyChanged(nameof(CutoffHint));

        // Required documents first, then the optional ones (the same two the API needs for the ✓ badge)
        Documents.Clear();
        VendorDocumentType[] required = [VendorDocumentType.BusinessRegistration, VendorDocumentType.FoodHygieneCertificate];
        foreach (var type in Enum.GetValues<VendorDocumentType>().OrderBy(t => required.Contains(t) ? 0 : 1))
        {
            Documents.Add(new ShopDocumentRow
            {
                Type = type,
                IsRequired = required.Contains(type) || shop.MissingDocuments.Contains(type),
                Document = shop.Documents.Where(d => d.Type == type).OrderByDescending(d => d.UploadedAt).FirstOrDefault()
            });
        }
    }

    [RelayCommand]
    private async Task Refresh() => await LoadAsync();

    // ---------- Details ----------
    [RelayCommand]
    private async Task Save()
    {
        if (Shop is null) return;

        var name = (BusinessName ?? string.Empty).Trim();
        if (name.Length < 2)
        {
            ShowError("Please enter your shop name.");
            return;
        }

        var email = string.IsNullOrWhiteSpace(BusinessEmail) ? null : BusinessEmail.Trim();
        var phone = string.IsNullOrWhiteSpace(BusinessPhone) ? null : BusinessPhone.Trim();
        await RunAsync(() => _seller.UpdateDetailsAsync(new UpdateShopDetailsCommand(name, email, phone)),
            Shop.Status == VendorStatus.Rejected
                ? "Saved. Phloem will look at your shop again."
                : "Shop details saved.");
    }

    // ---------- Pictures ----------
    [RelayCommand]
    private Task ChangeLogo() => ChangePictureAsync("Shop logo", _seller.ChangeLogoAsync, "Logo updated.");

    [RelayCommand]
    private Task RemoveLogo() => RunAsync(() => _seller.RemoveLogoAsync(), "Logo removed.");

    [RelayCommand]
    private Task ChangeCoverPhoto() => ChangePictureAsync("Cover photo", _seller.ChangeCoverPhotoAsync, "Cover photo updated.");

    [RelayCommand]
    private Task RemoveCoverPhoto() => RunAsync(() => _seller.RemoveCoverPhotoAsync(), "Cover photo removed.");

    private async Task ChangePictureAsync(string title,
                                          Func<Stream, string, string, CancellationToken, Task<ApiResult<string>>> upload,
                                          string done)
    {
        if (_busy) return;
        FileResult? picture;
        try
        {
            var choice = await Shell.Current.DisplayActionSheetAsync(title, "Cancel", null,
                "📷  Take a photo", "🖼  Choose from gallery");
            if (choice?.Contains("Take") == true)
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    ShowError("This device can't take photos. Choose one from your gallery instead.");
                    return;
                }
                picture = await MediaPicker.Default.CapturePhotoAsync();
            }
            else if (choice?.Contains("gallery") == true)
            {
                picture = (await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 }))
                    ?.FirstOrDefault();
            }
            else
            {
                return;
            }
        }
        catch (PermissionException)
        {
            ShowError("Please allow Phloem to use the camera and photos in your phone settings.");
            return;
        }
        catch (Exception)
        {
            ShowError("Couldn't open the photos. Please try again.");
            return;
        }

        if (picture is null) return;
        await RunAsync(async () =>
        {
            await using var stream = await picture.OpenReadAsync();
            var type = string.IsNullOrWhiteSpace(picture.ContentType) ? "image/jpeg" : picture.ContentType;
            return await upload(stream, picture.FileName, type, CancellationToken.None);
        }, done);
    }

    // ---------- Delivery fee ----------
    [RelayCommand]
    private async Task SaveDeliveryFee()
    {
        var text = DeliveryFeeText ?? string.Empty;
        if (CurrencySymbol.Length > 0) text = text.Replace(CurrencySymbol, string.Empty);
        if (InternationalDeliveryViewModel.ParseDecimal(text.Trim()) is not { } fee || fee < 0)
        {
            ShowError("Please enter your delivery fee, e.g. 15");
            return;
        }
        if (_fee is not null && (fee < _fee.MinAllowed || fee > _fee.MaxAllowed))
        {
            ShowError(FeeRangeText);
            return;
        }

        await RunAsync(() => _seller.SetDeliveryFeeAsync(fee), "Delivery fee saved.");
    }

    // ---------- Order cut-off ----------
    [RelayCommand]
    private async Task SaveCutoff()
    {
        var cutoff = TimeOnly.FromTimeSpan(OrderCutoff);
        await RunAsync(() => _seller.SetOrderCutoffAsync(cutoff),
                       $"Orders for today now close at {DateTime.Today.Add(OrderCutoff):h:mm tt}.");
    }

    // ---------- Documents ----------
    [RelayCommand]
    private async Task UploadDocument(ShopDocumentRow row)
    {
        if (_busy) return;
        FileResult? file;
        try
        {
            file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = $"{row.Title} (PDF, JPEG or PNG, up to 10 MB)",
                FileTypes = DocumentTypes
            });
        }
        catch (Exception)
        {
            ShowError("Couldn't open your files. Please try again.");
            return;
        }
        if (file is null) return;

        // Certificates and IDs may expire: ask (optional)
        DateOnly? expires = null;
        if (row.Type is VendorDocumentType.FoodHygieneCertificate or VendorDocumentType.OwnerId)
        {
            var typed = await Shell.Current.DisplayPromptAsync("Expiry date",
                "When does it expire? Leave empty if it doesn't.", "OK", "Skip", "e.g. 31/12/2027", 10,
                Keyboard.Text);
            if (!string.IsNullOrWhiteSpace(typed))
            {
                if (!DateOnly.TryParseExact(typed.Trim(), ["dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd"],
                                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    ShowError("Please type the expiry date like 31/12/2027.");
                    return;
                }
                expires = date;
            }
        }

        await RunAsync(async () =>
        {
            await using var stream = await file.OpenReadAsync();
            var result = await _seller.UploadDocumentAsync(row.Type, expires, stream, file.FileName, ContentTypeOf(file));
            return result;
        }, $"{row.Title} uploaded. Phloem will check it soon.");
    }

    private static string ContentTypeOf(FileResult file)
    {
        if (!string.IsNullOrWhiteSpace(file.ContentType)) return file.ContentType;
        return Path.GetExtension(file.FileName).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            _ => "image/jpeg"
        };
    }

    /// <summary>"Your name and personal phone are in My account" link.</summary>
    [RelayCommand]
    private async Task EditPersonal() => await Shell.Current.GoToAsync("//account/editprofile");

    // ---------- Helpers ----------

    /// <summary>Runs one change on the API, then shows the shop as it is now.</summary>
    private async Task RunAsync(Func<Task<ApiResult>> action, string done)
    {
        if (_busy) return;
        _busy = true;
        IsLoading = true;
        try
        {
            var result = await action();
            if (result.IsFailure)
            {
                ShowError(result.Error!.DisplayMessage);
                return;
            }

            await LoadAsync();
            IsError = false;
            StatusMessage = done;
        }
        catch (Exception)
        {
            ShowError("That didn't work. Please check your connection and try again.");
        }
        finally
        {
            _busy = false;
            IsLoading = false;
        }
    }

    private void ShowError(string message)
    {
        IsError = true;
        StatusMessage = message;
    }
}
