using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Phloem.Settings;

namespace Phloem.ViewModels;

/// <summary>Makes a <see cref="Paging"/> for a list, with that list's settings. Register as a singleton.</summary>
public sealed class PagingFactory(IOptions<PagingSettings> settings)
{
    /// <summary>A pager for one list.</summary>
    /// <param name="list">Which list (<see cref="PagedLists"/>).</param>
    /// <param name="load">Loads <see cref="Paging.Page"/> with <see cref="Paging.PageSize"/> items, then calls
    /// <see cref="Paging.Update"/> with the total.</param>
    public Paging Create(string list, Func<Task> load) =>
        new(list, settings.Value.DefaultSizeFor(list), settings.Value.OptionsFor(list), load);
}

/// <summary>
/// One list's pages, shown by <c>PagerView</c>: <c>Show [20 ▾]   ◀  Page 2 of 7  ▶</c>.
/// <list type="bullet">
///   <item><description>The screen loads <see cref="Page"/> with <see cref="PageSize"/> items, then calls
///   <see cref="Update"/> with the total the API returned.</description></item>
///   <item><description>New search or filter: call <see cref="Reset"/> before loading (back to page 1).</description></item>
///   <item><description>The person's "Show" choice is remembered on the phone, per list.</description></item>
/// </list>
/// </summary>
public partial class Paging : ObservableObject
{
    private readonly string _preferenceKey;
    private readonly Func<Task> _load;
    private bool _busy;

    public Paging(string list, int defaultSize, IReadOnlyList<int> sizeOptions, Func<Task> load)
    {
        _preferenceKey = $"Paging.{list}.PageSize";
        _load = load;
        SizeOptions = sizeOptions;

        var saved = defaultSize;
        try { saved = Preferences.Default.Get(_preferenceKey, defaultSize); }
        catch (Exception) { /* no saved choice: use the default */ }
        PageSize = sizeOptions.Contains(saved) ? saved : defaultSize;
    }

    /// <summary>Raised after moving to another page or size (the pager scrolls the list back to its top).</summary>
    public event EventHandler? PageChanged;

    /// <summary>The "Show" choices, smallest first.</summary>
    public IReadOnlyList<int> SizeOptions { get; }

    /// <summary>1-based page being shown.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack), nameof(CanGoNext), nameof(PageNumberText), nameof(RangeText))]
    public partial int Page { get; set; } = 1;

    /// <summary>Items per page.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages), nameof(CanGoBack), nameof(CanGoNext), nameof(HasManyPages),
                              nameof(TotalPagesText), nameof(PageNumberText), nameof(RangeText))]
    public partial int PageSize { get; set; }

    /// <summary>Items in the whole list (from the API).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages), nameof(CanGoNext), nameof(HasManyPages), nameof(IsVisible),
                              nameof(TotalPagesText), nameof(RangeText))]
    public partial int TotalCount { get; set; }

    public int TotalPages => Math.Max(1, (TotalCount + PageSize - 1) / PageSize);
    public bool CanGoBack => Page > 1;
    public bool CanGoNext => Page < TotalPages;

    /// <summary>◀ ▶ show only when there's more than one page.</summary>
    public bool HasManyPages => TotalPages > 1;

    /// <summary>The pager shows once the list is longer than the smallest "Show" choice (so the choice can be changed back).</summary>
    public bool IsVisible => TotalCount > SizeOptions[0];

    public string PageNumberText => Page.ToString();
    public string TotalPagesText => TotalPages.ToString();

    /// <summary>"21–40 of 137".</summary>
    public string RangeText => TotalCount == 0
        ? string.Empty
        : $"{(Page - 1) * PageSize + 1}–{Math.Min(Page * PageSize, TotalCount)} of {TotalCount}";

    /// <summary>Back to page 1 (new search or filter). Doesn't load.</summary>
    public void Reset() => Page = 1;

    /// <summary>Called by the screen after loading a page, with the total the API returned.</summary>
    public void Update(int totalCount) => TotalCount = Math.Max(0, totalCount);

    [RelayCommand]
    private Task Previous() => GoToAsync(Page - 1);

    [RelayCommand]
    private Task Next() => GoToAsync(Page + 1);

    /// <summary>"Show": pick how many items per page (remembered on this phone).</summary>
    [RelayCommand]
    private async Task ChooseSize()
    {
        if (_busy) return;

        var labels = SizeOptions.Select(s => s == PageSize ? $"✓  {s} per page" : $"{s} per page").ToArray();
        var choice = await Shell.Current.DisplayActionSheetAsync("Items per page", "Cancel", null, labels);
        var index = Array.IndexOf(labels, choice);
        if (index < 0 || SizeOptions[index] == PageSize) return;

        PageSize = SizeOptions[index];
        try { Preferences.Default.Set(_preferenceKey, PageSize); }
        catch (Exception) { /* not remembered; still used now */ }

        Page = 1;
        await LoadAsync();
    }

    private async Task GoToAsync(int page)
    {
        if (_busy || page < 1 || page > TotalPages || page == Page) return;
        Page = page;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _busy = true;
        try
        {
            PageChanged?.Invoke(this, EventArgs.Empty);
            await _load();
        }
        catch (Exception)
        {
            // The screen shows why it couldn't load
        }
        finally
        {
            _busy = false;
        }
    }
}
