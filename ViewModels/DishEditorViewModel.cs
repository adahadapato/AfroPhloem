using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Phloem.Models;
using Phloem.Services;

namespace Phloem.ViewModels;

/// <summary>One group in the dish picker, e.g. "🇬🇭 Ghana" with its dishes.</summary>
public class DishGroup : List<CatalogDish>
{
    public string Name { get; }
    public string Icon { get; }
    public DishGroup(string name, string icon, IEnumerable<CatalogDish> dishes) : base(dishes)
    {
        Name = name;
        Icon = icon;
    }
}

/// <summary>
/// Add / edit a dish (Seller dashboard). The name comes from the fixed dish list, plus an
/// optional detail ("& Chicken"); the photo comes from the camera or gallery.
///   Add:  GoToAsync("dishedit")
///   Edit: GoToAsync("dishedit", new Dictionary&lt;string, object&gt; { ["Dish"] = dish })
/// </summary>
public partial class DishEditorViewModel : ObservableObject, IQueryAttributable
{
    private readonly MockDataService _data;
    private readonly SessionService _session;
    private readonly DishCatalogService _catalog;

    private Dish? _editing;          // null = adding a new dish
    private Vendor? _vendor;

    public DishEditorViewModel(MockDataService data, SessionService session, DishCatalogService catalog)
    {
        _data = data;
        _session = session;
        _catalog = catalog;
        _vendor = FindMyShop();
        BuildPickerGroups();
    }

    // ---------- Page state ----------
    public bool IsEditing => _editing is not null;
    public string PageTitle => IsEditing ? "Edit dish" : "Add a dish";
    public string PageSubtitle => _vendor is null ? "Your shop" : $"For {_vendor.Name}";

    /// <summary>Quick detail chips under the dish name.</summary>
    public IReadOnlyList<string> DetailChips { get; } = new[] { "& Chicken", "& Fish", "& Beef", "& Goat", "Spicy", "Vegetarian", "Large" };

    // ---------- Name (from the list) ----------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBaseDish), nameof(FullName), nameof(CategoryText), nameof(PreviewName))]
    public partial CatalogDish? BaseDish { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName), nameof(PreviewName))]
    public partial string Detail { get; set; } = string.Empty;

    public bool HasBaseDish => BaseDish is not null;
    public string FullName => BaseDish is null ? string.Empty
                              : string.IsNullOrWhiteSpace(Detail) ? BaseDish.Name : $"{BaseDish.Name} {Detail.Trim()}";
    public string CategoryText => BaseDish?.Category ?? string.Empty;

    // ---------- Photo ----------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPhoto), nameof(HasNoPhoto), nameof(PreviewImage))]
    public partial string? PhotoPath { get; set; }

    public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoPath);
    public bool HasNoPhoto => !HasPhoto;

    // ---------- Price, description, availability ----------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewPrice))]
    public partial string PriceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsAvailable { get; set; } = true;

    // ---------- Live preview card ----------
    public string PreviewName => HasBaseDish ? FullName : "Your dish name";
    public string PreviewPrice => TryGetPrice(out var p) ? $"£{p:0.00}" : "£0.00";
    public string PreviewVendor => _vendor?.Name ?? "Your shop";
    public string PreviewImage => HasPhoto ? PhotoPath! : "hero_food_photo.png";

    // ---------- Status banner ----------
    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsError { get; set; }

    // ---------- Dish picker (overlay) ----------
    public ObservableCollection<DishGroup> PickerGroups { get; } = new();

    [ObservableProperty]
    public partial bool IsPickerOpen { get; set; }

    [ObservableProperty]
    public partial string PickerSearch { get; set; } = string.Empty;

    partial void OnPickerSearchChanged(string value) => BuildPickerGroups();

    // "Can't find your dish?" → suggest a new name
    [ObservableProperty]
    public partial bool IsSuggesting { get; set; }

    [ObservableProperty]
    public partial string SuggestName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SuggestCategory { get; set; } = "Local Meals";

    // =================================================================

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Dish", out var value) && value is Dish dish)
            LoadForEdit(dish);
        query.Clear();
    }

    private void LoadForEdit(Dish dish)
    {
        _editing = dish;
        var (baseDish, detail) = _catalog.Split(dish.Name, _vendor?.Country);
        BaseDish = baseDish;
        Detail = detail;
        PriceText = dish.Price.ToString("0.00", CultureInfo.InvariantCulture);
        Description = dish.Description ?? string.Empty;
        IsAvailable = dish.IsAvailable;
        PhotoPath = string.IsNullOrWhiteSpace(dish.ImageUrl) ? null : dish.ImageUrl;

        if (baseDish is null)
        {
            IsError = true;
            StatusMessage = "This dish's name isn't in the dish list yet. Please choose the closest name.";
        }

        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(PageTitle));
    }

    private Vendor? FindMyShop()
    {
        var userId = _session.CurrentUser?.Id;
        return _data.Vendors.FirstOrDefault(v => userId != null && v.UserId == userId)
               ?? _data.Vendors.FirstOrDefault();   // demo fallback, same as the dashboard
    }

    private void BuildPickerGroups()
    {
        var search = PickerSearch?.Trim() ?? string.Empty;
        var myCountry = _vendor?.Country;

        var groups = _catalog.Dishes
            .Where(d => search.Length == 0 || d.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .GroupBy(d => d.Group)
            // seller's own country first, then the rest
            .OrderByDescending(g => g.Key == myCountry)
            .ThenBy(g => g.Key switch { "Ghana" => 0, "Nigeria" => 1, "West African" => 2, "Snacks" => 3, _ => 4 })
            .Select(g => new DishGroup(g.Key, g.First().GroupIcon, g.OrderBy(d => d.Name)));

        PickerGroups.Clear();
        foreach (var g in groups) PickerGroups.Add(g);
    }

    private bool TryGetPrice(out decimal price) =>
        decimal.TryParse(PriceText?.Replace("£", "").Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out price)
        || decimal.TryParse(PriceText?.Replace("£", "").Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out price);

    // ---------- Picker ----------
    [RelayCommand]
    private void OpenPicker()
    {
        PickerSearch = string.Empty;
        IsSuggesting = false;
        BuildPickerGroups();
        IsPickerOpen = true;
    }

    [RelayCommand]
    private void ClosePicker() => IsPickerOpen = false;

    [RelayCommand]
    private void PickDish(CatalogDish dish)
    {
        BaseDish = dish;
        IsPickerOpen = false;
        if (IsError) { IsError = false; StatusMessage = string.Empty; }
    }

    [RelayCommand]
    private void ToggleDetail(string chip) => Detail = Detail == chip ? string.Empty : chip;

    // ---------- Suggest a new name ----------
    [RelayCommand]
    private void StartSuggest()
    {
        SuggestName = PickerSearch;
        IsSuggesting = true;
    }

    [RelayCommand]
    private void ChooseSuggestCategory(string category) => SuggestCategory = category;

    [RelayCommand]
    private async Task SendSuggestion()
    {
        if (string.IsNullOrWhiteSpace(SuggestName))
        {
            await Shell.Current.DisplayAlert("Dish name", "Please type the name of your dish.", "OK");
            return;
        }

        _catalog.Suggest(SuggestName, SuggestCategory, _vendor?.Country ?? "West African",
                         _vendor?.Name ?? _session.CurrentUser?.FullName ?? "A seller");

        IsSuggesting = false;
        IsPickerOpen = false;
        IsError = false;
        StatusMessage = $"Thanks! \"{SuggestName.Trim()}\" was sent to Phloem for approval. " +
                        "Once approved you'll find it in the dish list.";
        SuggestName = string.Empty;
    }

    // ---------- Photo ----------
    [RelayCommand]
    private async Task TakePhoto()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Shell.Current.DisplayAlert("Camera", "This device can't take photos. Choose one from your gallery instead.", "OK");
                return;
            }
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is not null) PhotoPath = photo.FullPath;   // the backend will upload this later
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Camera permission", "Please allow Phloem to use the camera in your phone settings.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Couldn't take photo", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ChoosePhoto()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo is not null) PhotoPath = photo.FullPath;   // the backend will upload this later
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Couldn't open gallery", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task PhotoOptions()
    {
        var choice = await Shell.Current.DisplayActionSheet("Dish photo", "Cancel", null,
            "📷  Take a photo", "🖼  Choose from gallery");
        if (choice?.Contains("Take") == true) await TakePhoto();
        else if (choice?.Contains("gallery") == true) await ChoosePhoto();
    }

    [RelayCommand]
    private void RemovePhoto() => PhotoPath = null;

    // ---------- Save / delete ----------
    [RelayCommand]
    private async Task Save()
    {
        if (BaseDish is null)
        {
            IsError = true;
            StatusMessage = "Please choose a dish name from the list.";
            return;
        }
        if (!TryGetPrice(out var price) || price <= 0)
        {
            IsError = true;
            StatusMessage = "Please enter a price, e.g. 8.50";
            return;
        }
        if (!HasPhoto)
        {
            var go = await Shell.Current.DisplayAlert("No photo yet",
                "Dishes with a photo sell much better. Save without a photo?", "Save anyway", "Add photo");
            if (!go) { await PhotoOptions(); return; }
        }

        if (_editing is null)
        {
            if (_vendor is null)
            {
                IsError = true;
                StatusMessage = "Set up your seller shop first.";
                return;
            }

            _data.Dishes.Add(new Dish
            {
                Name = FullName,
                CatalogName = BaseDish.Name,
                Category = BaseDish.Category,
                Price = price,
                Rating = 0,
                Country = _vendor.Country,
                VendorName = _vendor.Name,
                VendorId = _vendor.Id,
                ImageUrl = PhotoPath ?? string.Empty,
                Description = Description.Trim(),
                IsAvailable = IsAvailable
            });
        }
        else
        {
            _editing.Name = FullName;
            _editing.CatalogName = BaseDish.Name;
            _editing.Category = BaseDish.Category;
            _editing.Price = price;
            _editing.ImageUrl = PhotoPath ?? string.Empty;
            _editing.Description = Description.Trim();
            _editing.IsAvailable = IsAvailable;
        }

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (_editing is null) return;
        var ok = await Shell.Current.DisplayAlert("Delete this dish?",
            $"{_editing.Name} will be removed from your shop.", "Delete", "Keep");
        if (!ok) return;

        _data.Dishes.Remove(_editing);
        await Shell.Current.GoToAsync("..");
    }
}
