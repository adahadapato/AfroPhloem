using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Phloem.Models;
using Phloem.Services;
using Phloem.Services.Api;
using Phloem.Services.Api.Contracts.Dtos;
using Phloem.Services.Api.Contracts.Enums;
using Phloem.Settings;

namespace Phloem.ViewModels;

/// <summary>One document row on Admin → shop details.</summary>
public sealed class AdminDocumentRow
{
    public required AdminShopDocumentDto Document { get; init; }

    public string Title => AdminShopDetailsViewModel.DocumentName(Document.Type);
    public string FileText => $"{Document.OriginalFileName}  ·  {Document.FileSizeBytes / 1024.0:0} KB  ·  " +
                              $"uploaded {Document.UploadedAt.ToLocalTime():d MMM yyyy}";
    public string StatusText => Document.IsExpired ? "Expired" : Document.Status switch
    {
        DocumentStatus.Pending => "Waiting for review",
        DocumentStatus.Accepted => "Accepted",
        DocumentStatus.Rejected => $"Rejected: {Document.RejectionReason}",
        _ => Document.Status.ToString()
    };
    public bool IsPending => Document.Status == DocumentStatus.Pending && !Document.IsExpired;
    public bool IsBad => Document.IsExpired || Document.Status == DocumentStatus.Rejected;
}

/// <summary>
/// Admin → one shop (route "adminshop?id=…"): logo and cover, details, its documents (open them through a short-lived
/// secure link, accept or reject each), what's missing for the ✓ badge, and Approve / Reject / ✓ / Block.
/// </summary>
public partial class AdminShopDetailsViewModel : ObservableObject, IQueryAttributable
{
    private readonly AdminService _admin;
    private readonly ApiSettings _api;
    private Guid _shopId;

    public AdminShopDetailsViewModel(AdminService admin, IOptions<ApiSettings> api)
    {
        _admin = admin;
        _api = api.Value;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasShop))]
    public partial AdminShopCard? Shop { get; set; }

    public bool HasShop => Shop is not null;

    [ObservableProperty]
    public partial string? CoverPhotoUrl { get; set; }

    public ObservableCollection<AdminDocumentRow> Documents { get; } = new();

    /// <summary>"Missing for ✓: Food hygiene certificate, Owner ID", or empty.</summary>
    [ObservableProperty]
    public partial string MissingText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsError { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var id) && Guid.TryParse(id?.ToString(), out var shopId)) _shopId = shopId;
    }

    /// <summary>Called each time the page appears.</summary>
    public async Task LoadAsync()
    {
        if (_shopId == Guid.Empty) return;

        IsBusy = true;
        try
        {
            var result = await _admin.GetShopAsync(_shopId);
            if (result.IsFailure)
            {
                ShowError(result.Error!.DisplayMessage);
                return;
            }

            var details = result.Value!;
            Shop = new AdminShopCard
            {
                Shop = details.Shop,
                LogoUrl = _api.ResolveFileUrl(details.LogoUrl) ?? Vendor.DefaultLogo
            };
            CoverPhotoUrl = _api.ResolveFileUrl(details.CoverPhotoUrl) ?? Vendor.DefaultCoverPhoto;

            Documents.Clear();
            foreach (var d in details.Documents) Documents.Add(new AdminDocumentRow { Document = d });

            MissingText = details.MissingDocuments.Count == 0
                ? "All documents needed for the ✓ badge are accepted."
                : "Missing for the ✓ badge: " + string.Join(", ", details.MissingDocuments.Select(DocumentName));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Opens the private document in the browser through a 10-minute secure link.</summary>
    [RelayCommand]
    private async Task OpenDocument(AdminDocumentRow row)
    {
        var link = await _admin.GetDocumentLinkAsync(_shopId, row.Document.Id);
        if (link.IsFailure)
        {
            ShowError(link.Error!.DisplayMessage);
            return;
        }

        var url = _api.ResolveFileUrl(link.Value) ?? link.Value!;
        await Launcher.Default.OpenAsync(new Uri(url));
    }

    [RelayCommand]
    private async Task AcceptDocument(AdminDocumentRow row) =>
        await DoAsync(() => _admin.ReviewDocumentAsync(_shopId, row.Document.Id, accept: true, reason: null),
                      $"{row.Title} accepted.");

    [RelayCommand]
    private async Task RejectDocument(AdminDocumentRow row)
    {
        var reason = await Shell.Current.DisplayPromptAsync("Reject document",
            "What's wrong with it? The seller sees this and can upload a new one.", "Reject", "Cancel",
            placeholder: "e.g. The certificate has expired");
        if (string.IsNullOrWhiteSpace(reason)) return;

        await DoAsync(() => _admin.ReviewDocumentAsync(_shopId, row.Document.Id, accept: false, reason.Trim()),
                      $"{row.Title} rejected.");
    }

    [RelayCommand]
    private async Task Approve() => await DoAsync(() => _admin.ApproveShopAsync(_shopId), "The shop is live.");

    [RelayCommand]
    private async Task Reject()
    {
        var reason = await Shell.Current.DisplayPromptAsync("Reject shop",
            "Why can't it be approved? The seller sees this and can fix it.", "Reject", "Cancel");
        if (string.IsNullOrWhiteSpace(reason)) return;

        await DoAsync(() => _admin.RejectShopAsync(_shopId, reason.Trim()), "The shop was rejected.");
    }

    [RelayCommand]
    private async Task ToggleVerified()
    {
        if (Shop is null) return;
        await DoAsync(() => _admin.SetVerifiedAsync(_shopId, !Shop.IsVerified),
                      Shop.IsVerified ? "✓ badge removed." : "✓ badge given.");
    }

    [RelayCommand]
    private async Task ToggleBlock()
    {
        if (Shop is null) return;
        var block = !Shop.IsBlocked;
        if (block && !await Shell.Current.DisplayAlertAsync("Block this shop?",
                "It will be hidden from buyers. Its owner can still use Phloem as a customer.", "Block", "Cancel"))
            return;

        await DoAsync(() => _admin.SetBlockedAsync(_shopId, block), block ? "Shop blocked." : "Shop unblocked.");
    }

    /// <summary>"Food hygiene certificate" etc.</summary>
    public static string DocumentName(VendorDocumentType type) => type switch
    {
        VendorDocumentType.BusinessRegistration => "Business registration",
        VendorDocumentType.FoodHygieneCertificate => "Food hygiene certificate",
        VendorDocumentType.OwnerId => "Owner's ID",
        _ => "Other document"
    };

    private async Task DoAsync(Func<Task<ApiResult>> action, string done)
    {
        IsBusy = true;
        try
        {
            var result = await action();
            if (result.IsFailure)
            {
                ShowError(result.Error!.DisplayMessage);
                return;
            }
        }
        finally
        {
            IsBusy = false;
        }

        await LoadAsync();
        IsError = false;
        StatusMessage = done;
    }

    private void ShowError(string message)
    {
        IsError = true;
        StatusMessage = message;
    }
}
