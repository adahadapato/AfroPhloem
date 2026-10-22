using Phloem.ViewModels;

namespace Phloem.Views.Controls;

/// <summary>
/// The pager at the bottom of a long list: ◀ Page x of y ▶ and "Show n ▾". It shows a <see cref="ViewModels.Paging"/>
/// and, when the page changes, scrolls the page's list back to its top (or to <see cref="ScrollTarget"/>).
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PagerView : ContentView
{
    public PagerView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty PagingProperty =
        BindableProperty.Create(nameof(Paging), typeof(Paging), typeof(PagerView), null,
            propertyChanged: OnPagingChanged);

    /// <summary>The list's pages.</summary>
    public Paging? Paging
    {
        get => (Paging?)GetValue(PagingProperty);
        set => SetValue(PagingProperty, value);
    }

    public static readonly BindableProperty ScrollTargetProperty =
        BindableProperty.Create(nameof(ScrollTarget), typeof(Element), typeof(PagerView));

    /// <summary>Where to scroll when the page changes (e.g. the list's title); null = the top of the page.</summary>
    public Element? ScrollTarget
    {
        get => (Element?)GetValue(ScrollTargetProperty);
        set => SetValue(ScrollTargetProperty, value);
    }

    private static void OnPagingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var pager = (PagerView)bindable;
        if (oldValue is Paging old) old.PageChanged -= pager.OnPageChanged;
        if (newValue is Paging paging) paging.PageChanged += pager.OnPageChanged;
        pager.Card.BindingContext = newValue;
    }

    // A little press, then the step
    async void OnPreviousTapped(object? sender, TappedEventArgs e)
    {
        if (Paging is not { CanGoBack: true } paging) return;
        await PressAsync(PreviousButton);
        await paging.PreviousCommand.ExecuteAsync(null);
    }

    async void OnNextTapped(object? sender, TappedEventArgs e)
    {
        if (Paging is not { CanGoNext: true } paging) return;
        await PressAsync(NextButton);
        await paging.NextCommand.ExecuteAsync(null);
    }

    async void OnSizeTapped(object? sender, TappedEventArgs e)
    {
        if (Paging is not { } paging) return;
        await PressAsync(SizeButton);
        await paging.ChooseSizeCommand.ExecuteAsync(null);
    }

    private static async Task PressAsync(VisualElement button)
    {
        try
        {
            await button.ScaleToAsync(0.88, 70, Easing.CubicOut);
            await button.ScaleToAsync(1.0, 110, Easing.CubicOut);
        }
        catch (Exception)
        {
            button.Scale = 1;   // animation isn't essential
        }
    }

    /// <summary>The new page starts at the top of the list, not where the old one was scrolled to.</summary>
    private async void OnPageChanged(object? sender, EventArgs e)
    {
        try
        {
            var scroll = FindScrollView();
            if (scroll is null) return;

            if (ScrollTarget is Element target)
                await scroll.ScrollToAsync(target, ScrollToPosition.Start, true);
            else
                await scroll.ScrollToAsync(0, 0, true);
        }
        catch (Exception)
        {
            // Scrolling isn't essential
        }
    }

    /// <summary>The nearest vertical ScrollView around this pager.</summary>
    private ScrollView? FindScrollView()
    {
        for (var parent = Parent; parent is not null; parent = parent.Parent)
            if (parent is ScrollView { Orientation: not ScrollOrientation.Horizontal } scroll)
                return scroll;
        return null;
    }
}
